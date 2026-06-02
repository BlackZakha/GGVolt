using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<LibraryItemDto> _items = new();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LibraryViewModel(IApiService api)
    {
        _api = api;
        _ = LoadLibraryAsync();
    }

    [RelayCommand]
    private async Task LoadLibraryAsync()
    {
        IsLoading = true; ErrorMessage = string.Empty;
        try
        {
            var dtos = await _api.GetLibraryAsync(_cts.Token);
            Items = new ObservableCollection<LibraryItemDto>(dtos.Select(d => new LibraryItemDto
            {
                Title = d.Title, Version = d.Version, StatusText = d.StatusText,
                Progress = d.Progress, ActionText = d.ActionText
            }));
        }
        catch (Exception ex) { ErrorMessage = $"Ошибка библиотеки: {ex.Message}"; }
        finally { IsLoading = false; }
    }

    public void Dispose() => _cts.Cancel();
}