using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public class TokenRefresher : ITokenRefresher
{
    private readonly HttpClient _authHttp; // ✅ Отдельный HttpClient БЕЗ AuthMessageHandler
    private readonly ITokenStorage _storage;
    private readonly ITokenAccessor _session;
    private readonly ILogger<TokenRefresher>? _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public TokenRefresher(
        HttpClient authHttp, 
        ITokenStorage storage, 
        ITokenAccessor session, 
        ILogger<TokenRefresher>? logger = null)
    {
        _authHttp = authHttp;
        _storage = storage;
        _session = session;
        _logger = logger;
    }

    public async Task<bool> TryRefreshTokenAsync(CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        
        try
        {
            var currentRefreshToken = _session.RefreshToken;
            
            if (string.IsNullOrEmpty(currentRefreshToken))
            {
                _logger?.LogWarning("⚠️ Refresh token отсутствует");
                return false;
            }

            _logger?.LogInformation("🔄 Попытка обновления токена...");
            
            var resp = await _authHttp.PostAsJsonAsync("auth/refresh", new RefreshTokenRequest(currentRefreshToken), ct);
            
            if (!resp.IsSuccessStatusCode)
            {
                var error = await resp.Content.ReadAsStringAsync(ct);
                _logger?.LogWarning("❌ Refresh failed: {StatusCode} - {Error}", resp.StatusCode, error);
                return false;
            }

            var tokens = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
            
            if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
            {
                _logger?.LogError("❌ Сервер вернул пустой токен после refresh");
                return false;
            }

            _logger?.LogInformation("✅ Токен успешно обновлён: {Token}...", 
                tokens.AccessToken.Substring(0, Math.Min(20, tokens.AccessToken.Length)));
            
            await _storage.SaveAsync(tokens);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "❌ Ошибка при refresh");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}