using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using GGVolt.Client;
using GGVolt.Client.Services;
using GGVolt.Client.ViewModels;
using System;
using GGVolt.Client.Views;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices(services =>
            {
                // Token
                services.AddSingleton<ITokenAccessor, TokenSession>();
                services.AddSingleton<ITokenStorage, TokenStorage>();
                
                // ✅ HTTP-клиент для auth-запросов (БЕЗ AuthMessageHandler)
                services.AddHttpClient("GGVoltAuth", c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000/api/v1/");
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                });

                // ✅ HTTP-клиент для API (С AuthMessageHandler)
                services.AddHttpClient("GGVoltApi", c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000/api/v1/");
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                })
                .AddHttpMessageHandler<AuthMessageHandler>();

                // ✅ TokenRefresher использует GGVoltAuth (без handler)
                services.AddHttpClient<ITokenRefresher, TokenRefresher>("GGVoltAuth");
                
                // ✅ AuthMessageHandler зависит от ITokenRefresher
                services.AddTransient<AuthMessageHandler>();

                // ✅ ApiService использует GGVoltApi (с handler)
                services.AddHttpClient<IApiService, ApiService>("GGVoltApi");
                
                // ✅ AuthService использует GGVoltApi (с handler) — но НЕ вызывает refresh сам
                services.AddHttpClient<IAuthService, AuthService>("GGVoltApi");
                
                services.AddSingleton<IDownloadService, DownloadService>();
                services.AddSingleton<IArchiveService, ArchiveService>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<StoreViewModel>();
                services.AddTransient<LibraryViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<AuthViewModel>();
                services.AddTransient<GameDetailViewModel>();

                services.AddSingleton<App>();
                services.AddTransient<MainWindow>();
            })
            .Build();

        App.InitializeServices(host.Services);
        
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}