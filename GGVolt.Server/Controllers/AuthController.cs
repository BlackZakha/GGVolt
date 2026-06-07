using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GGVolt.Core.Entities;
using GGVolt.Core.Constants;
using GGVolt.Infrastructure.Repositories;
using GGVolt.Server.DTOs;
using GGVolt.Server.Services;

namespace GGVolt.Server.Controllers;

[ApiController]
[Route(CoreConstants.Api.Auth)]
public class AuthController : ControllerBase
{
    private readonly IRepository<User> _userRepo;
    private readonly IJwtService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IRepository<User> userRepo, IJwtService jwt, ILogger<AuthController> logger)
        => (_userRepo, _jwt, _logger) = (userRepo, jwt, logger);

    [HttpPost("register")]
    [ProducesResponseType(200, Type = typeof(RegisterResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (req.Password.Length < CoreConstants.Limits.MinPasswordLength)
            return BadRequest(new { error = $"Пароль должен быть ≥ {CoreConstants.Limits.MinPasswordLength} символов" });

        // Используем GetQueryable() + EF Core AnyAsync
        if (await _userRepo.GetQueryable().AnyAsync(u => u.Email == req.Email.ToLowerInvariant(), ct))
            return Conflict(new { error = "Email уже зарегистрирован" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = req.Username.Trim(),
            Email = req.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user, ct);
        await _userRepo.SaveChangesAsync(ct); // Теперь доступен

        _logger.LogInformation("Пользователь {UserId} успешно зарегистрирован", user.Id);
        return Ok(new RegisterResponse(user.Id, user.Username, user.Email));
    }

    [HttpPost("login")]
    [ProducesResponseType(200, Type = typeof(AuthResponse))]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _userRepo.GetQueryable()
            .FirstOrDefaultAsync(u => u.Username == req.Username.ToLowerInvariant(), ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        {
            _logger.LogWarning("Попытка входа с неверными данными: {Username}", req.Username);
            return Unauthorized(new { error = "Неверный email или пароль" });
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token, "", 60));
    }
}