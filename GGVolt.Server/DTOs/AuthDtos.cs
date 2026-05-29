namespace GGVolt.Server.DTOs;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string RefreshToken, int ExpiresInMinutes);
public record RegisterResponse(Guid Id, string Username, string Email);