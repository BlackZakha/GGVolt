using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public interface IApiService
{
    Task<IEnumerable<CatalogItemDto>> GetCatalogAsync(CancellationToken ct = default);
    // Task<IEnumerable<LibraryItemDto>> GetLibraryAsync(CancellationToken ct = default);
}