using System;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public interface ITokenAccessor
{
    string? AccessToken { get; }
    string? RefreshToken { get; } // ✅ Новое свойство
    bool IsAuthenticated { get; }
    void SetTokens(string? accessToken, string? refreshToken);
    void ClearTokens();
}

public class TokenSession : ITokenAccessor
{
    private string? _accessToken;
    private string? _refreshToken;
    private readonly ILogger<TokenSession>? _logger;

    public TokenSession(ILogger<TokenSession>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("🔑 TokenSession создан");
    }

    public string? AccessToken => _accessToken;
    public string? RefreshToken => _refreshToken;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_accessToken);
    
    public void SetTokens(string? accessToken, string? refreshToken)
    {
        var oldAccess = _accessToken;
        var oldRefresh = _refreshToken;
        
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        
        var accessPreview = string.IsNullOrEmpty(accessToken) ? "НЕТ" : 
            accessToken.Length > 20 ? accessToken.Substring(0, 20) + "..." : accessToken;
        var refreshPreview = string.IsNullOrEmpty(refreshToken) ? "НЕТ" : 
            refreshToken.Length > 20 ? refreshToken.Substring(0, 20) + "..." : refreshToken;
        
        _logger?.LogInformation("🔑 Токены изменены:");
        _logger?.LogInformation("   Access: {Old} → {New}", 
            string.IsNullOrEmpty(oldAccess) ? "НЕТ" : oldAccess.Substring(0, 20) + "...",
            accessPreview);
        _logger?.LogInformation("   Refresh: {Old} → {New}", 
            string.IsNullOrEmpty(oldRefresh) ? "НЕТ" : oldRefresh.Substring(0, 20) + "...",
            refreshPreview);
        _logger?.LogInformation("✅ IsAuthenticated={Auth}", IsAuthenticated);
    }
    
    public void ClearTokens()
    {
        _accessToken = null;
        _refreshToken = null;
        _logger?.LogInformation("🗑️ Токены очищены, IsAuthenticated={Auth}", IsAuthenticated);
    }
}