namespace Identity.Contracts.Api.Responses;

/// <summary>
/// Ответ при входе пользователя
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    DateTime RefreshTokenExpiresAt,
    Guid SessionId);

