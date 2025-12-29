namespace AccessPermissions.Contracts.Enums;

/// <summary>
/// Действие разрешения (публичный enum для API и Events)
/// </summary>
public enum PermissionActionContract
{
    /// <summary>
    /// Управление проектами
    /// </summary>
    ManageProjects = 1,

    /// <summary>
    /// Управление задачами
    /// </summary>
    ManageTasks = 2,

    /// <summary>
    /// Управление пользователями
    /// </summary>
    ManageUsers = 3,

    /// <summary>
    /// Управление настройками тенанта
    /// </summary>
    ManageTenantSettings = 4,

    /// <summary>
    /// Просмотр отчетов
    /// </summary>
    ViewReports = 5,

    /// <summary>
    /// Управление разрешениями
    /// </summary>
    ManagePermissions = 6
}

