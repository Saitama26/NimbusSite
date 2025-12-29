namespace Identity.Contracts.Api.Responses;

/// <summary>
/// Ответ при обновлении токена
/// </summary>
public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    DateTime RefreshTokenExpiresAt);

