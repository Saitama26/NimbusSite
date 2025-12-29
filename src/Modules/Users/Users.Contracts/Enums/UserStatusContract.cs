namespace Users.Contracts.Enums;

/// <summary>
/// Статус пользователя (публичный enum для API и Events)
/// </summary>
public enum UserStatusContract
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

