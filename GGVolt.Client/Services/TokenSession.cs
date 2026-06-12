using System;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public interface ITokenAccessor
{
    string? AccessToken { get; }
    bool IsAuthenticated { get; }
    void SetToken(string? token);
}

public class TokenSession : ITokenAccessor
{
    private string? _token;
    private readonly ILogger<TokenSession>? _logger;

    public TokenSession(ILogger<TokenSession>? logger = null)
    {
        _logger = logger;
        _logger?.LogInformation("🔑 TokenSession создан");
    }

    public string? AccessToken => _token;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    
    public void SetToken(string? token)
    {
        var oldValue = _token;
        _token = token;
        
        var tokenPreview = string.IsNullOrEmpty(token) ? "НЕТ" : 
            token.Length > 20 ? token.Substring(0, 20) + "..." : token;
        
        _logger?.LogInformation("🔑 Токен изменён: {Old} → {New}", 
            string.IsNullOrEmpty(oldValue) ? "НЕТ" : oldValue.Substring(0, 20) + "...",
            tokenPreview);
        _logger?.LogInformation("✅ IsAuthenticated={Auth}", IsAuthenticated);
    }
}