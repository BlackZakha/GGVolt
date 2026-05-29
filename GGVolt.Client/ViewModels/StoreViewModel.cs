using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using GGVolt.Client.Models;

namespace GGVolt.Client.ViewModels;

public partial class StoreViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<CatalogItem> _items = new();

    public StoreViewModel()
    {
        // Демо-данные (позже заменим на вызов API)
        Items = new ObservableCollection<CatalogItem>
        {
            new() { Title = "VoltSync", Subtitle = "Синхронизация файлов v2.1", Progress = 82, ActionText = "Запустить" },
            new() { Title = "PixelForge", Subtitle = "Инди-платформер", Progress = 0, ActionText = "Скачать" },
            new() { Title = "NetProbe", Subtitle = "Диагностика сети", Progress = 0, ActionText = "Скачать" },
            new() { Title = "AeroUI Kit", Subtitle = "Библиотека интерфейсов", Progress = 0, ActionText = "Скачать" },
            new() { Title = "CyberCore: Overload", Subtitle = "Системный утилит", Progress = 0, ActionText = "Скачать бесплатно" },
            new() { Title = "ShaderGen Pro", Subtitle = "Генератор шейдеров", Progress = 45, ActionText = "Обновить" },
        };
    }
}