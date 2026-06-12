using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.ViewModels;

public partial class StoreViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly CancellationTokenSource _cts = new();
    private readonly Action<Guid>? _onOpenGameDetail;

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<GameDto> _items = new();
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private ContentType? _selectedType;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasItems => Items.Count > 0;

    public StoreViewModel(IApiService api, Action<Guid>? onOpenGameDetail = null)
    {
        _api = api;
        _onOpenGameDetail = onOpenGameDetail;
        _ = LoadCatalogAsync();
    }

    [RelayCommand]
    private async Task LoadCatalogAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _api.GetCatalogAsync(CurrentPage, 20, SelectedType, _cts.Token);
            Items = new ObservableCollection<GameDto>(result.Items);
            TotalPages = (int)Math.Ceiling(result.TotalCount / (double)result.PageSize);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void OpenGameDetail(GameDto game)
    {
        _onOpenGameDetail?.Invoke(game.Id);
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCatalogAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCatalogAsync();
        }
    }
}