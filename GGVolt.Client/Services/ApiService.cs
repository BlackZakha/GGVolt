using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http) => _http = http;

    public async Task<IEnumerable<CatalogItemDto>> GetCatalogAsync(CancellationToken ct = default)
    {
        // Ожидает JSON: [{ "id":1, "title":"...", ... }]
        var items = await _http.GetFromJsonAsync<List<CatalogItemDto>>("/catalog", ct);
        return items ?? Enumerable.Empty<CatalogItemDto>();
    }

    public async Task<IEnumerable<LibraryItemDto>> GetLibraryAsync(CancellationToken ct = default)
    {
        var items = await _http.GetFromJsonAsync<List<LibraryItemDto>>("/library", ct);
        return items ?? Enumerable.Empty<LibraryItemDto>();
    }
}