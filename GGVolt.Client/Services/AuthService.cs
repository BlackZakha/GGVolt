using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ITokenStorage _storage;
    private readonly ITokenAccessor _session;

    public AuthService(HttpClient http, ITokenStorage storage, ITokenAccessor session)
    {
        _http = http;
        _storage = storage;
        _session = session;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync() => await _storage.LoadAsync();

    // ✅ Свойства делегируют к сессии — MainWindowViewModel работает без изменений
    public bool IsAuthenticated => _session.IsAuthenticated;
    public string? AccessToken => _session.AccessToken;

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("/api/v1/auth/login", req, ct);
        resp.EnsureSuccessStatusCode();
        var tokens = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
        if (tokens != null) await _storage.SaveAsync(tokens); // TokenStorage сам обновит сессию
        return tokens;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("/api/v1/auth/register", req, ct);
        resp.EnsureSuccessStatusCode();
        var tokens = await resp.Content.ReadFromJsonAsync<AuthResponse>(ct);
        if (tokens != null) await _storage.SaveAsync(tokens);
        return tokens;
    }

    public async Task LogoutAsync()
    {
        await _storage.ClearAsync(); // TokenStorage сам очистит сессию
    }
}