using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GGVolt.Core.Entities;
using GGVolt.Core.Enums;
using GGVolt.Core.Constants;
using GGVolt.Infrastructure.Repositories;
using GGVolt.Server.DTOs;
using Microsoft.Extensions.Logging;

namespace GGVolt.Server.Controllers;

[ApiController]
[Route(CoreConstants.Api.Games)]
public class GamesController : ControllerBase
{
    private readonly IRepository<Game> _gameRepo;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IRepository<Game> gameRepo, ILogger<GamesController> logger)
        => (_gameRepo, _logger) = (gameRepo, logger);

    [HttpGet]
    [ProducesResponseType(200, Type = typeof(PagedResponse<GameDto>))]
    public async Task<IActionResult> GetGames(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = CoreConstants.Limits.DefaultPageSize,
        [FromQuery] ContentType? type = default,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, CoreConstants.Limits.MaxPageSize);

        // 1. Базовый запрос строго типа IQueryable<Game>
        IQueryable<Game> query = _gameRepo.GetQueryable()
            .Where(g => g.IsVisible);

        // 2. Динамическая фильтрация
        if (type.HasValue)
            query = query.Where(g => g.ContentType == type.Value);

        // 3. CountAsync лучше вызывать ДО сортировки (немного быстрее)
        var total = await query.CountAsync(ct);

        // 4. Сортировка, пагинация и проекция в одной цепочке
        var items = await query
            .OrderBy(g => g.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GameDto(
                g.Id, g.Title, g.Description, g.Price, g.CoverUrl, g.ReleaseDate, g.ContentType))
            .ToListAsync(ct);

        return Ok(new PagedResponse<GameDto>(items, total, page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200, Type = typeof(GameDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetGame(Guid id, CancellationToken ct)
    {
        var game = await _gameRepo.GetByIdAsync(id, ct);
        if (game == null || !game.IsVisible)
            return NotFound(new { error = "Продукт не найден или скрыт" });

        return Ok(new GameDto(game.Id, game.Title, game.Description, game.Price, game.CoverUrl, game.ReleaseDate, game.ContentType));
    }
}