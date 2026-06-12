using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GGVolt.Client.Models.Api;
using GGVolt.Client.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
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
    [ObservableProperty] private GameDetailDto? _game;
    [ObservableProperty] private bool _isOwned;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsFree => Game?.Price == 0;
    public bool HasScreenshots => Game?.Screenshots != null && Game.Screenshots.Count > 0; // ✅ Новое свойство
    public bool HasSystemRequirements => Game?.SystemRequirements != null;
    public string PurchaseButtonText => IsOwned ? "В библиотеке" : IsFree ? "Получить бесплатно" : $"Купить за {Game?.Price} ₽";
    public bool CanPurchase => !IsOwned && !IsLoading;

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

        try
        {
            _logger.LogInformation("🎮 Загрузка деталей игры {GameId}", gameId);
            Game = await _api.GetGameDetailAsync(gameId);
            
            if (_auth.IsAuthenticated)
            {
                var library = await _api.GetLibraryAsync();
                IsOwned = library.Any(g => g.Id == gameId);
            }
            
            _logger.LogInformation("✅ Игра загружена: {Title}", Game.Title);
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
            OnPropertyChanged(nameof(HasScreenshots)); // ✅ Уведомляем UI
            OnPropertyChanged(nameof(HasSystemRequirements));
        }
    }

    [RelayCommand]
    private void GoBack() => _onNavigateBack?.Invoke();

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        if (IsOwned || Game == null) return;

        try
        {
            _logger.LogInformation("💳 Покупка игры {Title}", Game.Title);
            // TODO: Реализовать API покупки
            ErrorMessage = "Функция покупки в разработке";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при покупке");
            ErrorMessage = $"Ошибка покупки: {ex.Message}";
        }
    }
}