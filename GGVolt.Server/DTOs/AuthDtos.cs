using System.ComponentModel.DataAnnotations;

namespace GGVolt.Server.DTOs;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(
    [Required, MinLength(3), MaxLength(32)] string Username, 
    [Required, MinLength(6)] string Password);
public record AuthResponse(string Token, string RefreshToken, int ExpiresInMinutes);
public record RegisterResponse(Guid Id, string Username, string Email);