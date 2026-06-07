using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.Services;

public class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenAccessor _token;
    
    public AuthMessageHandler(ITokenAccessor token) => _token = token;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Добавляем Bearer, если токен есть и заголовок ещё не установлен
        if (_token.IsAuthenticated && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // При 401 — очищаем токен (сессия протухла)
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _token.SetToken(null);
        }

        return response;
    }
}