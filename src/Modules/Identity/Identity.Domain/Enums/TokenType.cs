namespace Identity.Domain.Enums;

/// <summary>
/// Тип токена аутентификации
/// </summary>
public enum TokenType
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

