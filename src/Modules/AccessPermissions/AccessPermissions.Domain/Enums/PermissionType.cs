namespace AccessPermissions.Domain.Enums;

/// <summary>
/// Тип разрешения (что разрешено делать)
/// </summary>
public enum PermissionType
{
    /// <summary>
    /// Чтение (просмотр)
    /// </summary>
    Read = 1,

    /// <summary>
    /// Создание
    /// </summary>
    Create = 2,

    /// <summary>
    /// Обновление/редактирование
    /// </summary>
    Update = 3,

    /// <summary>
    /// Удаление
    /// </summary>
    Delete = 4,

    /// <summary>
    /// Полный доступ (все действия)
    /// </summary>
    FullAccess = 5
}

