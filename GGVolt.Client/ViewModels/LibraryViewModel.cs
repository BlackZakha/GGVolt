using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace GGVolt.Client.ViewModels;
public partial class LibraryViewModel : ViewModelBase
{
    public ObservableCollection<LibraryItem> LibraryItems { get; } = new()
    {
        new() { Title = "CyberCore", Version = "v2.4.1", StatusText = "Готово", Progress = 100, ActionText = "Запустить" },
        new() { Title = "VoltSync", Version = "v1.0.0", StatusText = "Обновление...", IsUpdating = true, Progress = 45, ActionText = "Остановить" },
        new() { Title = "PixelForge", Version = "v0.9.2", StatusText = "Установлено", Progress = 100, ActionText = "Запустить" }
    };
}

public class LibraryItem
{
    public string Title { get; set; } = "";
    public string Version { get; set; } = "";
    public string StatusText { get; set; } = "";
    public double Progress { get; set; }
    public bool IsUpdating { get; set; }
    public string ActionText { get; set; } = "Запустить";
}