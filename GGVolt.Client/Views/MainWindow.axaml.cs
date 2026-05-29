using Avalonia.Controls;
using Avalonia.Interactivity;
using GGVolt.Client.ViewModels;

namespace GGVolt.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void CloseClickButton(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MaximizeClickButton(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Maximized;
    }

    private void MinimizeClickButton(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
}