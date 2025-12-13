namespace Contracts.Identity;

/// <summary>
/// Контракт для обмена данными о сессии между модулями
/// </summary>
public sealed record SessionContract(
    Guid Id,
    Guid UserId,
    Guid TenantId,
    SessionStatusContract Status,
    DateTime CreatedAt,
    DateTime LastActivityAt,
    DateTime ExpiresAt,
    string? IpAddress = null,
    string? UserAgent = null);

/// <summary>
/// Контракт для обмена данными о токене аутентификации
/// </summary>
public sealed record TokenContract(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    DateTime RefreshTokenExpiresAt);

/// <summary>
/// Статус сессии
/// </summary>
public enum SessionStatusContract
{
    Active = 0,
    Closed = 1,
    Expired = 2,
    Revoked = 3
}

/// <summary>
/// Тип токена аутентификации
/// </summary>
public enum TokenTypeContract
{
    /// <summary>
    /// Access токен (краткосрочный)
    /// </summary>
    Access = 0,

    /// <summary>
    /// Refresh токен (долгосрочный)
    /// </summary>
    Refresh = 1,

    /// <summary>
    /// Токен для сброса пароля
    /// </summary>
    PasswordReset = 2,

    /// <summary>
    /// Токен для подтверждения email
    /// </summary>
    EmailConfirmation = 3
}

