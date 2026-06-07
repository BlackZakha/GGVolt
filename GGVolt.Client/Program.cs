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
    public static void Main(string[] args) // ✅ void, а не int
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        // 1. Настройка хоста и DI
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITokenAccessor, TokenSession>();
                services.AddSingleton<ITokenStorage, TokenStorage>();
                services.AddTransient<AuthMessageHandler>();
                
                // Единый HTTP-клиент для всех сервисов
                services.AddHttpClient("GGVoltApi", c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000/api/v1/");
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                })
                .AddHttpMessageHandler<AuthMessageHandler>();

                // Регистрация сервисов
                services.AddHttpClient<IApiService, ApiService>("GGVoltApi");
                services.AddHttpClient<IAuthService, AuthService>("GGVoltApi");
                
                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<StoreViewModel>();
                services.AddTransient<LibraryViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<AuthViewModel>();

                // App & MainWindow
                services.AddSingleton<App>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        // 2. Инициализация приложения
        App.InitializeServices(host.Services);
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}