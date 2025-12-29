namespace Users.Contracts.Enums;

/// <summary>
/// Роль пользователя (публичный enum для API и Events)
/// </summary>
public enum UserRoleContract
{
    /// <summary>
    /// Администратор
    /// </summary>
    Admin = 1,

    /// <summary>
    /// Менеджер
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Обычный пользователь
    /// </summary>
    User = 3,

    /// <summary>
    /// Гость (ограниченный доступ)
    /// </summary>
    Guest = 4
}

