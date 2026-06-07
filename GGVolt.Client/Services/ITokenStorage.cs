using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public interface ITokenStorage
{
    Task SaveAsync(AuthResponse tokens);
    Task<AuthResponse?> LoadAsync();
    Task ClearAsync();
}