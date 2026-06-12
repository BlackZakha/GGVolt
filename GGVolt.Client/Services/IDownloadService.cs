using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Download;

namespace GGVolt.Client.Services;

public interface IDownloadService
{
    IReadOnlyList<DownloadItem> ActiveDownloads { get; }
    event EventHandler<DownloadItem>? DownloadStarted;
    event EventHandler<DownloadItem>? DownloadProgressChanged;
    event EventHandler<DownloadItem>? DownloadCompleted;
    event EventHandler<DownloadItem>? DownloadFailed;

    Task<DownloadItem> StartDownloadAsync(Guid gameId, string title, string version, string url, string installPath, long totalBytes, CancellationToken ct = default);
    Task PauseDownloadAsync(int downloadId);
    Task ResumeDownloadAsync(int downloadId);
    Task CancelDownloadAsync(int downloadId);
}