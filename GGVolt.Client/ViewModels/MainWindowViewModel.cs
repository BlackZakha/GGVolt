using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage = null!;
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public StoreViewModel StorePage { get; }
    public LibraryViewModel LibraryPage { get; }
    public SettingsViewModel SettingsPage { get; } = new();
    public AuthViewModel AuthPage { get; }
    public GameDetailViewModel GameDetailPage { get; }

    private readonly IAuthService _auth;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private bool _isAuthenticated;
    [ObservableProperty] private bool _isNotAuthenticated;

    public MainWindowViewModel(IApiService api, IAuthService auth, IDownloadService downloadService, ILogger<LibraryViewModel> logger, IServiceProvider serviceProvider)
    {
        System.Diagnostics.Debug.WriteLine("MainWindowViewModel: создание...");
        
        _auth = auth;
        IsAuthenticated = auth.IsAuthenticated;
        IsNotAuthenticated = !_isAuthenticated;
        _serviceProvider = serviceProvider;
        System.Diagnostics.Debug.WriteLine($"IsAuthenticated: {IsAuthenticated}");

        AuthPage = new AuthViewModel(auth);
        // ✅ Исправленная сигнатура EventHandler
        AuthPage.AuthCompleted += OnAuthCompleted;

        StorePage = new StoreViewModel(api);
        System.Diagnostics.Debug.WriteLine("StoreViewModel создан");
        
        LibraryPage = new LibraryViewModel(api, auth, downloadService, logger);
        System.Diagnostics.Debug.WriteLine("LibraryViewModel создан");
        
        GameDetailPage = new GameDetailViewModel(
            api, auth, 
            serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GameDetailViewModel>>(),
            OnNavigateBack);
        
        CurrentPage = StorePage;
        System.Diagnostics.Debug.WriteLine("✅ MainWindowViewModel инициализирован");
    }

    private void OnAuthCompleted(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("✅ Авторизация успешна");
        IsAuthenticated = true;
        IsNotAuthenticated = false;
        
        LibraryPage.OnUserAuthenticated();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        System.Diagnostics.Debug.WriteLine("Выход из аккаунта");
        await _auth.LogoutAsync();
        AuthPage.NotifyLogout();
        IsAuthenticated = false;
        IsNotAuthenticated = true;
    }

    [RelayCommand] private void NavigateToStore() 
    {
        System.Diagnostics.Debug.WriteLine("Переход в Магазин");
        CurrentPage = StorePage;
    }
    
    [RelayCommand] private void NavigateToLibrary() 
    {
        System.Diagnostics.Debug.WriteLine("Переход в Библиотеку");
        if (_auth.IsAuthenticated && LibraryPage.Items.Count == 0)
        {
            LibraryPage.OnUserAuthenticated();
        }
        
        CurrentPage = LibraryPage;
    }
    
    [RelayCommand] private void NavigateToSettings() 
    {
        System.Diagnostics.Debug.WriteLine("Переход в Настройки");
        CurrentPage = SettingsPage;
    }
    
    private void OnOpenGameDetail(Guid gameId)
    {
        _ = GameDetailPage.LoadGameAsync(gameId);
        CurrentPage = GameDetailPage;
    }

    private void OnNavigateBack()
    {
        CurrentPage = StorePage;
    }
}