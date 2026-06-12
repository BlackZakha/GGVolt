using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _storage;
    private readonly ITokenAccessor _session;
    private readonly ILogger<AuthService>? _logger;

    public AuthService(HttpClient http, ITokenStorage storage, ITokenAccessor session, ILogger<AuthService>? logger = null)
    {
        _http = http;
        _storage = storage;
        _session = session;
        _logger = logger;
        
        // ✅ Токен уже загружен синхронно в TokenStorage
        _logger?.LogInformation("🔐 AuthService создан, IsAuthenticated={Auth}", _session.IsAuthenticated);
    }

    public bool IsAuthenticated => _session.IsAuthenticated;
    public string? AccessToken => _session.AccessToken;

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        _logger?.LogInformation("🔐 Попытка входа для {Username}", req.Username);
        
        var resp = await _http.PostAsJsonAsync("auth/login", req, ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync(ct);
            _logger?.LogWarning("❌ Login failed: {StatusCode} - {Error}", resp.StatusCode, error);
            throw new HttpRequestException($"Login failed: {resp.StatusCode} - {error}");
        }

        var tokens = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
        
        if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken))
        {
            _logger?.LogError("❌ Сервер вернул пустой токен!");
            throw new InvalidOperationException("Сервер не вернул access token");
        }

        _logger?.LogInformation("✅ Вход успешен, токен получен: {Token}...", 
            tokens.AccessToken.Substring(0, Math.Min(20, tokens.AccessToken.Length)));
        
        await _storage.SaveAsync(tokens);
        return tokens;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        _logger?.LogInformation("📝 Регистрация пользователя {Username}", req.Username);
        
        var regResp = await _http.PostAsJsonAsync("auth/register", req, ct);
        if (!regResp.IsSuccessStatusCode)
        {
            var error = await regResp.Content.ReadAsStringAsync(ct);
            _logger?.LogWarning("❌ Register failed: {StatusCode} - {Error}", regResp.StatusCode, error);
            throw new HttpRequestException($"Register failed: {regResp.StatusCode} - {error}");
        }

        _logger?.LogInformation("✅ Регистрация успешна, выполняем вход");
        return await LoginAsync(new LoginRequest(req.Username, req.Password), ct);
    }

    public async Task LogoutAsync()
    {
        _logger?.LogInformation("🚪 Выход из системы");
        await _storage.ClearAsync();
    }
}