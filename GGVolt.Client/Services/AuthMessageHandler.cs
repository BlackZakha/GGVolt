using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenAccessor _token;
    private readonly ITokenRefresher _refresher; // ✅ Зависит от ITokenRefresher, а не IAuthService
    private readonly ILogger<AuthMessageHandler>? _logger;
    
    public AuthMessageHandler(ITokenAccessor token, ITokenRefresher refresher, ILogger<AuthMessageHandler>? logger = null)
    {
        _token = token;
        _refresher = refresher;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_token.IsAuthenticated && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // ✅ При 401 пытаемся refresh (но не для auth endpoints)
        if (response.StatusCode == HttpStatusCode.Unauthorized && 
            !request.RequestUri?.AbsolutePath.Contains("/auth/") == true)
        {
            _logger?.LogWarning("🚫 401 для {Uri}, пытаемся refresh...", request.RequestUri);
            
            var refreshed = await _refresher.TryRefreshTokenAsync(cancellationToken);
            
            if (refreshed)
            {
                _logger?.LogInformation("🔄 Повторяем запрос с новым токеном");
                
                var newRequest = await CloneRequestAsync(request);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
                
                response.Dispose();
                return await base.SendAsync(newRequest, cancellationToken);
            }
            else
            {
                _logger?.LogError("❌ Refresh не удался, очищаем сессию");
                _token.ClearTokens();
            }
        }

        return response;
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        
        return clone;
    }
}