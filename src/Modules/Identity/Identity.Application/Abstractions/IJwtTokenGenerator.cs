namespace Identity.Application.Abstractions;

/// <summary>
/// Сервис для генерации JWT токенов
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Сгенерировать access token
    /// </summary>
    string GenerateAccessToken(Guid userId, int tenantId, IEnumerable<string>? roles = null);

    /// <summary>
    /// Сгенерировать refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Получить время истечения access token
    /// </summary>
    DateTime GetAccessTokenExpiration();

    /// <summary>
    /// Получить время истечения refresh token
    /// </summary>
    DateTime GetRefreshTokenExpiration();
}

