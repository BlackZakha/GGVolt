using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GGVolt.Client.ViewModels;

public partial class GameDetailViewModel : ViewModelBase
{
    private readonly IApiService _api;
    private readonly IAuthService _auth;
    private readonly ILogger<GameDetailViewModel> _logger;
    private readonly Action? _onNavigateBack;

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;
    [ObservableProperty] private GameDetailDto? _game;
    [ObservableProperty] private bool _isOwned;
    [ObservableProperty] private bool _isPurchasing;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool IsFree => Game?.Price == 0;
    public bool HasScreenshots => Game?.Screenshots != null && Game.Screenshots.Count > 0;
    public bool HasSystemRequirements => Game?.SystemRequirements != null;
    
    public string PurchaseButtonText => IsOwned 
        ? "✓ В библиотеке" 
        : IsFree 
            ? "Получить бесплатно" 
            : $"Купить за {Game?.Price} ₽";
    
    public bool CanPurchase => !IsOwned && !IsLoading && !IsPurchasing;

    public GameDetailViewModel(IApiService api, IAuthService auth, ILogger<GameDetailViewModel> logger, Action? onNavigateBack = null)
    {
        _api = api;
        _auth = auth;
        _logger = logger;
        _onNavigateBack = onNavigateBack;
    }

    public async Task LoadGameAsync(Guid gameId)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            _logger.LogInformation("🎮 Загрузка деталей игры {GameId}", gameId);
            Game = await _api.GetGameDetailAsync(gameId);
            
            if (_auth.IsAuthenticated)
            {
                var library = await _api.GetLibraryAsync();
                IsOwned = library.Any(g => g.Id == gameId);
            }
            
            _logger.LogInformation("✅ Игра загружена: {Title}, IsOwned={Owned}", Game.Title, IsOwned);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при загрузке деталей игры");
            ErrorMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(PurchaseButtonText));
            OnPropertyChanged(nameof(CanPurchase));
        }
    }

    [RelayCommand]
    private void GoBack() => _onNavigateBack?.Invoke();

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (IsOwned || Game == null || !_auth.IsAuthenticated) return;

        IsPurchasing = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            _logger.LogInformation(" Покупка игры {Title} (бесплатно={Free})", Game.Title, IsFree);

            // Для платных игр — показываем подтверждение
            bool confirm = IsFree;
            if (!IsFree)
            {
                // В реальном приложении здесь открывается окно платёжной системы
                // Пока используем простое подтверждение через диалог
                // В Avalonia можно использовать ContentDialog или MessageBox
                confirm = true; // TODO: заменить на реальный диалог
            }

            if (!confirm)
            {
                _logger.LogInformation("Покупка отменена пользователем");
                return;
            }

            var result = await _api.PurchaseGameAsync(Game.Id, confirm);
            
            IsOwned = true;
            SuccessMessage = $"✓ Игра \"{result.Title}\" добавлена в вашу библиотеку!";
            
            _logger.LogInformation("✅ Игра успешно приобретена: {Title}", result.Title);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            IsOwned = true;
            SuccessMessage = "✓ Игра уже в вашей библиотеке";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при покупке");
            ErrorMessage = $"Ошибка покупки: {ex.Message}";
        }
        finally
        {
            IsPurchasing = false;
            OnPropertyChanged(nameof(PurchaseButtonText));
            OnPropertyChanged(nameof(CanPurchase));
        }
    }
}