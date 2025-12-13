namespace AccessPermissions.Domain.Enums;

/// <summary>
/// Область действия разрешения (на что распространяется)
/// </summary>
public enum PermissionScope
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

