namespace Application.Abstractions.Auth;

/// <summary>
/// Сервис для управления refresh токенами
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Сохранить refresh token для пользователя
    /// </summary>
    Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по refresh token
    /// </summary>
    Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Инвалидировать refresh token
    /// </summary>
    Task InvalidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Инвалидировать все refresh токены пользователя
    /// </summary>
    Task InvalidateAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, валиден ли refresh token
    /// </summary>
    Task<bool> IsRefreshTokenValidAsync(string refreshToken, CancellationToken cancellationToken = default);
}

