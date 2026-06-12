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
    private readonly ILogger<AuthMessageHandler>? _logger;
    
    public AuthMessageHandler(ITokenAccessor token, ILogger<AuthMessageHandler>? logger = null)
    {
        _token = token;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger?.LogDebug("📤 Запрос: {Method} {Uri}", request.Method, request.RequestUri);
        _logger?.LogDebug("🔐 IsAuthenticated: {Auth}, Token: {Token}", 
            _token.IsAuthenticated, 
            _token.AccessToken != null ? _token.AccessToken.Substring(0, 10) + "..." : "null");

        if (_token.IsAuthenticated && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
            _logger?.LogInformation("✅ Добавлен Bearer токен к запросу {Uri}", request.RequestUri);
        }
        else if (!_token.IsAuthenticated)
        {
            _logger?.LogWarning("⚠️ Запрос без токена: {Uri}", request.RequestUri);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger?.LogWarning("🚫 Получен 401 Unauthorized для {Uri}, очищаем токен", request.RequestUri);
            _token.SetToken(null);
        }

        return response;
    }
}