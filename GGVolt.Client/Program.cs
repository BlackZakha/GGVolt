using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GGVolt.Client;
using GGVolt.Client.Services;
using GGVolt.Client.ViewModels;
using System;
using GGVolt.Client.Views;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        // 1. Настройка DI
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // HTTP-клиент (укажите реальный URL бэкенда)
                services.AddHttpClient<IApiService, ApiService>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000/api/v1/");
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                });

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<StoreViewModel>();
                services.AddTransient<LibraryViewModel>();
                services.AddTransient<SettingsViewModel>();

                // App & MainWindow
                services.AddSingleton<App>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        // 2. Инициализация
        App.InitializeServices(host.Services);
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}