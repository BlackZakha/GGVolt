using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Models.Download;
using GGVolt.Client.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Helpers;

namespace GGVolt.Client.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    private readonly IDownloadService _downloadService;
    private readonly ILogger<LibraryViewModel> _logger;
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<GameDto> _items = new();
    [ObservableProperty] private ObservableCollection<DownloadItem> _activeDownloads = new();
    [ObservableProperty] private bool _hasActiveDownloads;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasItems => Items.Count > 0;

    public LibraryViewModel(
        IApiService api, 
        IAuthService auth, 
        IDownloadService downloadService,
        ILogger<LibraryViewModel> logger)
    {
        _api = api;
        _auth = auth;
        _downloadService = downloadService;
        _logger = logger;

        _logger.LogInformation("📚 LibraryViewModel создан, IsAuthenticated={Auth}", _auth.IsAuthenticated);

        ActiveDownloads.CollectionChanged += (_, _) => HasActiveDownloads = ActiveDownloads.Count > 0;

        _downloadService.DownloadStarted += (_, d) => MainThreadHelper.ExecuteOnUIThread(() => ActiveDownloads.Add(d));
        _downloadService.DownloadProgressChanged += (_, d) => MainThreadHelper.ExecuteOnUIThread(() =>
        {
            var ex = ActiveDownloads.FirstOrDefault(x => x.Id == d.Id);
            if (ex != null) { ex.Progress = d.Progress; ex.StatusText = d.StatusText; ex.SpeedMbps = d.SpeedMbps; ex.RemainingTime = d.RemainingTime; }
        });
        _downloadService.DownloadCompleted += (_, d) => MainThreadHelper.ExecuteOnUIThread(() => ActiveDownloads.Remove(d));
    }

    [RelayCommand]
    private async Task LoadLibraryAsync()
    {
        _logger.LogInformation("🔄 LoadLibraryAsync вызван, IsAuthenticated={Auth}", _auth.IsAuthenticated);
    
        if (!_auth.IsAuthenticated)
        {
            _logger.LogWarning("⚠️ Пользователь не авторизован!");
            ErrorMessage = "Требуется авторизация";
            return;
        }

        _logger.LogInformation("✅ Начинаем загрузку библиотеки...");
        IsLoading = true;
        ErrorMessage = string.Empty;
    
        try
        {
            var items = await _api.GetLibraryAsync(_cts.Token);
            _logger.LogInformation("📦 Загружено {Count} элементов", items.Count);
        
            Items.Clear();
            foreach (var item in items) Items.Add(item);
        
            ErrorMessage = string.Empty;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError(ex, "❌ 401 Unauthorized - сессия истекла");
            ErrorMessage = "Сессия истекла. Пожалуйста, войдите снова.";
            // Можно добавить кнопку "Войти снова" или автоматически перенаправить на страницу входа
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при загрузке библиотеки");
            ErrorMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _logger.LogInformation("🏁 LoadLibraryAsync завершен");
        }
    }

    [RelayCommand]
    private async Task StartDownloadAsync(GameDto game)
    {
        try
        {
            _logger.LogInformation("⬇️ Начало загрузки {Title}", game.Title);
            var link = await _api.GetDownloadLinkAsync(game.Id, _cts.Token);
            _logger.LogInformation("✅ Получена ссылка для загрузки");

            var installPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GGVolt", "Games", game.Title);

            await _downloadService.StartDownloadAsync(
                game.Id,
                game.Title,
                game.Type.ToString(),
                link.SignedUrl,
                installPath,
                link.SizeBytes,
                _cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при начале загрузки");
            ErrorMessage = $"Не удалось начать загрузку: {ex.Message}";
        }
    }

    [RelayCommand] private Task PauseDownloadAsync(DownloadItem d) => _downloadService.PauseDownloadAsync(d.Id);
    [RelayCommand] private Task ResumeDownloadAsync(DownloadItem d) => _downloadService.ResumeDownloadAsync(d.Id);
    [RelayCommand]
    private async Task CancelDownloadAsync(DownloadItem d)
    {
        await _downloadService.CancelDownloadAsync(d.Id);
        ActiveDownloads.Remove(d);
    }

    public void OnUserAuthenticated()
    {
        _logger.LogInformation("🔔 OnUserAuthenticated вызван");
        _ = LoadLibraryAsync();
    }
    
    public void Dispose() => _cts.Cancel();
}