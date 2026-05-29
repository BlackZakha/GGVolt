using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace GGVolt.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    public StoreViewModel StorePage { get; } = new();
    public LibraryViewModel LibraryPage { get; } = new();
    public SettingsViewModel SettingsPage { get; } = new();

    [RelayCommand]
    private void NavigateToStore() => CurrentPage = StorePage;

    [RelayCommand]
    private void NavigateToLibrary() => CurrentPage = LibraryPage;

    [RelayCommand]
    private void NavigateToSettings() => CurrentPage = SettingsPage;

    public MainWindowViewModel()
    {
        // Страница по умолчанию
        CurrentPage = StorePage;
    }
}