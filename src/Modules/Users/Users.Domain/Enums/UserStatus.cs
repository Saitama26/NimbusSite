namespace Users.Domain.Enums;

/// <summary>
/// Статус пользователя
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Активен
    /// </summary>
    Active = 1,

    /// <summary>
    /// Неактивен (заблокирован)
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// Удален (soft delete)
    /// </summary>
    Deleted = 3
}

