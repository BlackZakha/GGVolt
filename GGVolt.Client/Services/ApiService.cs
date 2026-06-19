using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;
using Microsoft.Extensions.Logging;

namespace GGVolt.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly ILogger? _logger;

    public ApiService(HttpClient http, ILogger<ApiService>? logger = null)
    {
        _http = http;
        _logger = logger;
    } 

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
    
        // ✅ Логируем полученный URL
        System.Diagnostics.Debug.WriteLine($"🔗 Получен DownloadLink: SignedUrl='{result?.SignedUrl}', Size={result?.SizeBytes}");
    
        if (result == null || string.IsNullOrEmpty(result.SignedUrl))
        {
            throw new InvalidOperationException("Сервер вернул пустой signedUrl");
        }
    
        return result ?? throw new InvalidOperationException("Пустой ответ от сервера");
    }
    
    public async Task<GameDetailDto> GetGameDetailAsync(Guid gameId, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"games/{gameId}", ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GameDetailDto>(ct);
        return result ?? throw new InvalidOperationException("Сервер вернул пустой ответ");
    }
    
    public async Task<PurchaseResponse> PurchaseGameAsync(Guid gameId, bool confirmPayment, CancellationToken ct = default)
    {
        var url = $"games/{gameId}/purchase";
        _logger?.LogInformation("📤 POST {Url} с confirmPayment={Confirm}", url, confirmPayment);
    
        var response = await _http.PostAsJsonAsync(url, new PurchaseRequest(confirmPayment), ct);
    
        var responseContent = await response.Content.ReadAsStringAsync(ct);
        _logger?.LogInformation("📥 Ответ: {StatusCode} - {Content}", response.StatusCode, responseContent);
    
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Purchase failed: {response.StatusCode} - {responseContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<PurchaseResponse>(ct);
        return result ?? throw new InvalidOperationException("Пустой ответ от сервера");
    }
}