using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Client.Models.Api;

namespace GGVolt.Client.Services;

public interface IApiService
{
    Task<PagedResponse<GameDto>> GetCatalogAsync(int page = 1, int pageSize = 20, ContentType? type = null, CancellationToken ct = default);
    Task<GameDetailDto> GetGameDetailAsync(Guid gameId, CancellationToken ct = default);
    Task<List<GameDto>> GetLibraryAsync(CancellationToken ct = default);
    Task<DownloadLinkResponse> GetDownloadLinkAsync(Guid gameId, CancellationToken ct = default);
}