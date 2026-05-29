using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using GGVolt.Core.Entities;
using GGVolt.Core.Constants;
using GGVolt.Core.Enums;
using GGVolt.Infrastructure.Data;
using GGVolt.Infrastructure.Repositories;
using GGVolt.Server.DTOs;

namespace GGVolt.Server.Controllers;

[ApiController]
[Route(CoreConstants.Api.Library)]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly IRepository<License> _licenseRepo;
    private readonly GGVoltDbContext _dbContext;
    private readonly ILogger<LibraryController> _logger;

    public LibraryController(IRepository<License> licenseRepo, GGVoltDbContext dbContext, ILogger<LibraryController> logger)
        => (_licenseRepo, _dbContext, _logger) = (licenseRepo, dbContext, logger);

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(List<GameDto>))]
    public async Task<IActionResult> GetMyGames(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var owned = await _dbContext.Licenses
            .AsNoTracking()
            .Where(l => l.UserId == userId && l.Status == LicenseStatus.Active)
            .Select(l => new GameDto(
                l.Game.Id,
                l.Game.Title,
                l.Game.Description,
                l.Game.Price,
                l.Game.CoverUrl,
                l.Game.ReleaseDate,
                l.Game.ContentType))
            .ToListAsync(ct);

        return Ok(owned);
    }
}