namespace Identity.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление токена
/// </summary>
public sealed record RefreshTokenRequest(
    string RefreshToken);

