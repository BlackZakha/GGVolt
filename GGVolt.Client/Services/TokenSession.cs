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
    public string? AccessToken => _token;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public void SetToken(string? token) => _token = token;
}