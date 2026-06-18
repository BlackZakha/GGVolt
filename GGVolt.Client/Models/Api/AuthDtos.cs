using System;
using System.Text.Json.Serialization;

namespace GGVolt.Client.Models.Api;

public record LoginRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password
);

public record RegisterRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);

// ✅ Добавляем RefreshTokenRequest
public record RefreshTokenRequest(
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

public record AuthResponse(
    [property: JsonPropertyName("token")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("expiresIn")] int ExpiresIn
);

public record RegisterResponse(
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("email")] string Email
);