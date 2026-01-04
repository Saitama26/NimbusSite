namespace Users.Domain.Enums;

/// <summary>
/// Роль пользователя
/// </summary>
public enum UserRole
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

