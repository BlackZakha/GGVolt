using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GGVolt.Core.Entities;
using GGVolt.Core.Constants;
using GGVolt.Core.Enums;
using GGVolt.Infrastructure.Data;
using GGVolt.Server.DTOs;

namespace GGVolt.Server.Controllers;

[ApiController]
[Route(CoreConstants.Api.Games)]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly GGVoltDbContext _dbContext;
    private readonly ILogger<PurchaseController> _logger;

    public PurchaseController(GGVoltDbContext dbContext, ILogger<PurchaseController> logger)
        => (_dbContext, _logger) = (dbContext, logger);

    [HttpPost("{gameId:guid}/purchase")]
    [ProducesResponseType(200, Type = typeof(PurchaseResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Purchase(Guid gameId, [FromBody] PurchaseRequest? request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        _logger.LogInformation("📥 POST /api/v1/games/{GameId}/purchase от пользователя {UserId}", gameId, userId);

        var game = await _dbContext.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == gameId && g.IsVisible, ct);

        if (game == null)
        {
            _logger.LogWarning("❌ Игра {GameId} не найдена", gameId);
            return NotFound(new { error = "Игра не найдена" });
        }

        var alreadyOwned = await _dbContext.Licenses
            .AsNoTracking()
            .AnyAsync(l => l.UserId == userId && l.GameId == gameId && l.Status == LicenseStatus.Active, ct);

        if (alreadyOwned)
        {
            _logger.LogWarning("⚠️ Игра {GameId} уже куплена пользователем {UserId}", gameId, userId);
            return Conflict(new { error = "Игра уже в вашей библиотеке" });
        }

        if (game.Price > 0 && (request == null || !request.ConfirmPayment))
        {
            return BadRequest(new { error = "Требуется подтверждение оплаты" });
        }

        var license = new License
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GameId = gameId,
            Status = LicenseStatus.Active,
            PurchasedAt = DateTime.UtcNow,
            PricePaid = game.Price
        };

        _dbContext.Licenses.Add(license);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("✅ Пользователь {UserId} получил игру {GameId}", userId, gameId);

        return Ok(new PurchaseResponse(gameId, game.Title, game.Price));
    }
}