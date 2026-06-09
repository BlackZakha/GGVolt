using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// JWT конфигурация
var key = Encoding.UTF8.GetBytes("super-secret-key-for-ggvolt-mock-server-12345");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 🟦 AUTH
app.MapPost("/api/v1/auth/login", (LoginRequest req) =>
{
    if (req.Username == "admin" && req.Password == "admin")
    {
        var token = GenerateToken(req.Username);
        return Results.Ok(new { access_token = token, refresh_token = "mock-refresh", expires_in = 3600, user_id = 1 });
    }
    return Results.Unauthorized();
});

app.MapPost("/api/v1/auth/register", (RegisterRequest req) =>
{
    var token = GenerateToken(req.Username);
    return Results.Ok(new { access_token = token, refresh_token = "mock-refresh", expires_in = 3600, user_id = 1 });
});

// 🟦 CATALOG
app.MapGet("/api/v1/catalog", () =>
{
    return new[]
    {
        new { id = 1, title = "CyberCore", subtitle = "Системный утилит", type = "software", progress = 0, action_text = "Скачать" },
        new { id = 2, title = "PixelForge", subtitle = "Инди-платформер", type = "game", progress = 75, action_text = "Обновить" },
        new { id = 3, title = "VoltSync", subtitle = "Синхронизация файлов", type = "software", progress = 0, action_text = "Скачать" }
    };
});

// 🟦 LIBRARY (требует авторизации)
app.MapGet("/api/v1/library", () =>
{
    return new[]
    {
        new { id = 1, title = "CyberCore", version = "v2.4.1", status_text = "Готово", progress = 100, action_text = "Запустить" },
        new { id = 2, title = "VoltSync", version = "v1.0.0", status_text = "Обновление...", progress = 45, action_text = "Остановить" }
    };
}).RequireAuthorization();

app.Run();

string GenerateToken(string username)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

record LoginRequest(string Username, string Password);
record RegisterRequest(string Username, string Email, string Password);