using System.Threading;
using System.Threading.Tasks;

namespace GGVolt.Client.Services;

public interface ITokenRefresher
{
    Task<bool> TryRefreshTokenAsync(CancellationToken ct = default);
}