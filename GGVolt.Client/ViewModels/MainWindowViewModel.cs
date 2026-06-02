using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Services;

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

    public MainWindowViewModel(IApiService api)
    {
        // DI передаёт IApiService, мы создаём дочерние VM
        StorePage = new StoreViewModel(api);
        LibraryPage = new LibraryViewModel(api);
        CurrentPage = StorePage;
    }

    [RelayCommand] private void NavigateToStore() => CurrentPage = StorePage;
    [RelayCommand] private void NavigateToLibrary() => CurrentPage = LibraryPage;
    [RelayCommand] private void NavigateToSettings() => CurrentPage = SettingsPage;
}