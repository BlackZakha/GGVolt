using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GGVolt.Core.Entities;
using GGVolt.Core.Constants;
using GGVolt.Core.Enums;
using GGVolt.Infrastructure.Data;
using GGVolt.Infrastructure.Storage;
using GGVolt.Server.DTOs;

namespace GGVolt.Server.Controllers;

[ApiController]
[Route(CoreConstants.Api.Downloads)]
[Authorize]
public class DownloadController : ControllerBase
{
    private readonly GGVoltDbContext _dbContext;
    private readonly IFileStorageService _storage;
    private readonly ILogger<DownloadController> _logger;

    public DownloadController(GGVoltDbContext dbContext, IFileStorageService storage, ILogger<DownloadController> logger)
        => (_dbContext, _storage, _logger) = (dbContext, storage, logger);

    [HttpGet("{gameId:guid}")]
    [ProducesResponseType(200, Type = typeof(DownloadLinkResponse))]
    [ProducesResponseType(403, Type = typeof(object))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDownloadLink(Guid gameId, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var hasLicense = await _dbContext.Licenses
            .AsNoTracking()
            .AnyAsync(l => l.UserId == userId && l.GameId == gameId && l.Status == LicenseStatus.Active, ct);

        // Forbid() не принимает объект. Для API лучше вернуть StatusCode(403, ...)
        if (!hasLicense)
            return StatusCode(403, new { error = "У вас нет лицензии на этот продукт" });

        var game = await _dbContext.Games.FindAsync(new object[] { gameId }, ct);
        if (game == null || string.IsNullOrEmpty(game.StorageKey))
            return NotFound(new { error = "Продукт не найден или не имеет контента" });

        // Генерируем реальную временную ссылку через MinIO
        var expiry = TimeSpan.FromMinutes(15);
        var signedUrl = await _storage.GeneratePresignedUrlAsync(game.StorageKey, expiry, ct);
        
        _logger.LogInformation("Пользователь {UserId} получил ссылку для {GameId} (истекает через {Expiry})", 
            userId, gameId, expiry);
            
        return Ok(new DownloadLinkResponse(signedUrl, expiry, game.SizeBytes));
    }
}