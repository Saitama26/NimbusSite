namespace AccessPermissions.Contracts.Enums;

/// <summary>
/// Область действия разрешения (публичный enum для API и Events)
/// </summary>
public enum PermissionScopeContract
{
    /// <summary>
    /// На уровне тенанта (все проекты тенанта)
    /// </summary>
    Tenant = 1,

    /// <summary>
    /// На уровне проекта (конкретный проект)
    /// </summary>
    Project = 2,

    /// <summary>
    /// На уровне задачи (конкретная задача)
    /// </summary>
    Task = 3
}

