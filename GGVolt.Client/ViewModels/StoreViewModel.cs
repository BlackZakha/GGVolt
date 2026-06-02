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

public partial class StoreViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly CancellationTokenSource _cts = new();

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<CatalogItemDto> _items = new();

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public StoreViewModel(IApiService api)
    {
        _api = api;
        _ = LoadCatalogAsync();
    }

    [RelayCommand]
    private async Task LoadCatalogAsync()
    {
        if (_cts.IsCancellationRequested) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var dtos = await _api.GetCatalogAsync(_cts.Token);
            Items = new ObservableCollection<CatalogItemDto>(
                dtos.Select(d => new CatalogItemDto
                {
                    Title = d.Title, Subtitle = d.Subtitle,
                    Progress = d.Progress, ActionText = d.ActionText
                }));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    public void Dispose() => _cts.Cancel();
}