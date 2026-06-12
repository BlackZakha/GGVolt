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
                
                // HTTP
                services.AddTransient<AuthMessageHandler>();
                services.AddHttpClient("GGVoltApi", c =>
                {
                    c.BaseAddress = new Uri("http://localhost:5000/api/v1/");
                    c.DefaultRequestHeaders.Add("Accept", "application/json");
                })
                .AddHttpMessageHandler<AuthMessageHandler>();

                services.AddHttpClient<IApiService, ApiService>("GGVoltApi");
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

                // App
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