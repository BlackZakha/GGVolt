using System;
using System.Collections.Generic;
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

    public async Task<PagedResponse<GameDto>> GetCatalogAsync(int page = 1, int pageSize = 20, ContentType? type = null, CancellationToken ct = default)
    {
        var url = $"games?page={page}&pageSize={pageSize}";
        if (type.HasValue) url += $"&type={(int)type.Value}";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<GameDto>>(ct);
        return result ?? new PagedResponse<GameDto>(new List<GameDto>(), 0, page, pageSize);
    }

    public async Task<List<GameDto>> GetLibraryAsync(CancellationToken ct = default)
    {
        var response = await _http.GetAsync("library", ct);
        response.EnsureSuccessStatusCode();

        var items = await response.Content.ReadFromJsonAsync<List<GameDto>>(ct);
        return items ?? new List<GameDto>();
    }

    public async Task<DownloadLinkResponse> GetDownloadLinkAsync(Guid gameId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"downloads/{gameId}", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<DownloadLinkResponse>(ct);
        return result ?? throw new InvalidOperationException("Сервер вернул пустой ответ");
    }
    
    public async Task<GameDetailDto> GetGameDetailAsync(Guid gameId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"games/{gameId}", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GameDetailDto>(ct);
        return result ?? throw new InvalidOperationException("Сервер вернул пустой ответ");
    }
}