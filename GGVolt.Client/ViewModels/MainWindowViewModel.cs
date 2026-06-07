using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Services;
using System;
using System.Threading.Tasks;

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

    private readonly IAuthService _auth;

    [ObservableProperty] private bool _isAuthenticated;
    public bool IsNotAuthenticated => !_isAuthenticated;

    public MainWindowViewModel(IApiService api, IAuthService auth)
    {
        _auth = auth;
        IsAuthenticated = auth.IsAuthenticated;

        AuthPage = new AuthViewModel(auth);
        // ✅ Исправленная сигнатура EventHandler
        AuthPage.AuthCompleted += OnAuthCompleted;

        StorePage = new StoreViewModel(api);
        LibraryPage = new LibraryViewModel(api);
        CurrentPage = StorePage;
    }

    private void OnAuthCompleted(object? sender, EventArgs e) => IsAuthenticated = true;

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        AuthPage.NotifyLogout();
        IsAuthenticated = false;
    }

    [RelayCommand] private void NavigateToStore() => CurrentPage = StorePage;
    [RelayCommand] private void NavigateToLibrary() => CurrentPage = LibraryPage;
    [RelayCommand] private void NavigateToSettings() => CurrentPage = SettingsPage;
}