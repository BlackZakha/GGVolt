using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GGVolt.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using GGVolt.Client.Views;

namespace GGVolt.Client;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;

    public static void InitializeServices(IServiceProvider services) => Services = services;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // DI резолвит MainWindow + MainWindowViewModel
            var mainWindow = Services.GetRequiredService<MainWindow>();
            desktop.MainWindow = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
}