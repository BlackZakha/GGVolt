using CommunityToolkit.Mvvm.ComponentModel;

namespace GGVolt.Client.ViewModels;
public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty] private string _username = "PlayerGG";
    [ObservableProperty] private string _email = "player@ggvolt.ru";
    [ObservableProperty] private string _downloadPath = @"C:\GGVolt\Games";
    [ObservableProperty] private int _speedLimit = 0;
    [ObservableProperty] private bool _minimizeToTray = true;
}