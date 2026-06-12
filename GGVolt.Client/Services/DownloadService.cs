using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Enums;
using GGVolt.Client.Models.Download;

namespace GGVolt.Client.Services;

public class DownloadService : IDownloadService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IArchiveService _archiveService;
    private readonly Dictionary<int, DownloadItem> _downloads = new();
    private readonly SemaphoreSlim _downloadSemaphore = new(2, 2);
    private int _nextId = 1;
    private bool _disposed;

    public IReadOnlyList<DownloadItem> ActiveDownloads => _downloads.Values.ToList();
    
    public event EventHandler<DownloadItem>? DownloadStarted;
    public event EventHandler<DownloadItem>? DownloadProgressChanged;
    public event EventHandler<DownloadItem>? DownloadCompleted;
    public event EventHandler<DownloadItem>? DownloadFailed;
    public event EventHandler<DownloadItem>? InstallationStarted;
    public event EventHandler<DownloadItem>? InstallationCompleted;

    public DownloadService(HttpClient httpClient, IArchiveService archiveService)
    {
        _httpClient = httpClient;
        _archiveService = archiveService;
    }

    public async Task<DownloadItem> StartDownloadAsync(Guid gameId, string title, string version, string url, string installPath, long totalBytes, CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(DownloadService));
        
        await _downloadSemaphore.WaitAsync(ct);

        var download = new DownloadItem
        {
            GameId = gameId,
            Id = Interlocked.Increment(ref _nextId),
            Title = title,
            Version = version,
            DownloadUrl = url,
            InstallPath = installPath,
            TotalBytes = totalBytes,
            ZipPath = Path.Combine(installPath, $"{title}.zip"),
            Status = DownloadStatus.Queued,
            StatusText = "В очереди..."
        };

        _downloads[download.Id] = download;
        _ = DownloadAndInstallAsync(download, ct);

        DownloadStarted?.Invoke(this, download);
        return download;
    }

    private async Task DownloadAndInstallAsync(DownloadItem download, CancellationToken externalCt)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        download.CancellationTokenSource = cts;

        try
        {
            // === ЭТАП 1: ЗАГРУЗКА ===
            download.Status = DownloadStatus.Downloading;
            download.StatusText = "Загрузка...";

            var response = await _httpClient.GetAsync(download.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                download.Status = DownloadStatus.Failed;
                download.StatusText = $"Ошибка: {response.StatusCode}";
                download.ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                DownloadFailed?.Invoke(this, download);
                return;
            }

            download.TotalBytes = response.Content.Headers.ContentLength ?? 0;

            using var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            Directory.CreateDirectory(download.InstallPath);

            using var fileStream = new FileStream(download.ZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            var lastProgressUpdate = Stopwatch.StartNew();
            var lastDownloadedBytes = 0L;

            while (true)
            {
                if (download.Status == DownloadStatus.Paused)
                {
                    await Task.Delay(100, cts.Token);
                    continue;
                }

                var bytesRead = await stream.ReadAsync(buffer, cts.Token);
                if (bytesRead == 0) break;

                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cts.Token);
                download.DownloadedBytes += bytesRead;

                if (lastProgressUpdate.ElapsedMilliseconds >= 100)
                {
                    var elapsed = lastProgressUpdate.Elapsed.TotalSeconds;
                    var bytesDelta = download.DownloadedBytes - lastDownloadedBytes;
                    
                    download.SpeedMbps = bytesDelta / elapsed / (1024 * 1024);
                    download.Progress = download.Percentage;
                    download.RemainingTime = download.SpeedMbps > 0 ? 
                        TimeSpan.FromSeconds((download.TotalBytes - download.DownloadedBytes) / (download.SpeedMbps * 1024 * 1024)) : 
                        TimeSpan.Zero;

                    DownloadProgressChanged?.Invoke(this, download);
                    
                    lastProgressUpdate.Restart();
                    lastDownloadedBytes = download.DownloadedBytes;
                }
            }

            // === ЭТАП 2: РАСПАКОВКА ===
            download.Status = DownloadStatus.Extracting;
            download.StatusText = "Распаковка...";
            download.Progress = 0;
            download.SpeedMbps = 0;
            download.RemainingTime = TimeSpan.Zero;
            
            InstallationStarted?.Invoke(this, download);

            _archiveService.ProgressChanged += (_, progress) =>
            {
                download.Progress = progress;
                DownloadProgressChanged?.Invoke(this, download);
            };

            await _archiveService.ExtractZipAsync(download.ZipPath, download.InstallPath, cts.Token);

            // Удаляем ZIP после успешной распаковки
            await _archiveService.DeleteZipAsync(download.ZipPath);

            download.Status = DownloadStatus.Completed;
            download.StatusText = "Установлено";
            download.Progress = 100;
            
            DownloadCompleted?.Invoke(this, download);
            InstallationCompleted?.Invoke(this, download);
        }
        catch (OperationCanceledException) when (download.Status == DownloadStatus.Paused)
        {
            // Нормальная пауза
        }
        catch (Exception ex)
        {
            if (download.Status != DownloadStatus.Failed)
            {
                download.Status = DownloadStatus.Failed;
                download.StatusText = $"Ошибка: {ex.Message}";
                download.ErrorMessage = ex.Message;
                DownloadFailed?.Invoke(this, download);
            }
        }
        finally
        {
            try { cts?.Dispose(); } catch { }
            download.CancellationTokenSource = null;
            _downloadSemaphore.Release();
        }
    }

    public Task PauseDownloadAsync(int downloadId)
    {
        if (_downloads.TryGetValue(downloadId, out var download) && 
            download.Status == DownloadStatus.Downloading)
        {
            download.Status = DownloadStatus.Paused;
            download.StatusText = "Приостановлено";
        }
        return Task.CompletedTask;
    }

    public Task ResumeDownloadAsync(int downloadId)
    {
        if (_downloads.TryGetValue(downloadId, out var download) && 
            download.Status == DownloadStatus.Paused)
        {
            download.Status = DownloadStatus.Downloading;
            download.StatusText = "Загрузка...";
        }
        return Task.CompletedTask;
    }

    public async Task CancelDownloadAsync(int downloadId)
    {
        if (_downloads.TryGetValue(downloadId, out var download))
        {
            try
            {
                if (download.CancellationTokenSource != null && 
                    !download.CancellationTokenSource.IsCancellationRequested)
                {
                    download.CancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Ошибка при отмене: {ex.Message}");
            }
            
            await Task.Delay(100);
            _downloads.Remove(downloadId);
        }
    }

    public Task RemoveDownloadAsync(int downloadId)
    {
        _downloads.Remove(downloadId);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        
        _downloadSemaphore.Dispose();
        
        foreach (var download in _downloads.Values)
        {
            try
            {
                download.CancellationTokenSource?.Cancel();
                download.CancellationTokenSource?.Dispose();
            }
            catch { }
        }
        
        _downloads.Clear();
    }
}