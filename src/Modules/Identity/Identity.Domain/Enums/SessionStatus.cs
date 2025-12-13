namespace Identity.Domain.Enums;

/// <summary>
/// Статус сессии пользователя
/// </summary>
public enum SessionStatus
{
    /// <summary>
    /// Активная сессия
    /// </summary>
    Active = 0,

    /// <summary>
    /// Сессия закрыта пользователем
    /// </summary>
    Closed = 1,

    /// <summary>
    /// Сессия истекла
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Сессия отозвана (например, при смене пароля)
    /// </summary>
    Revoked = 3
}

