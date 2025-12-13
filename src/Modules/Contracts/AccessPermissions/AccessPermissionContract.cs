namespace Contracts.AccessPermissions;

/// <summary>
/// Контракт для обмена данными о разрешении доступа между модулями
/// </summary>
public sealed record AccessPermissionContract(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid? ProjectId,
    Guid? TaskId,
    PermissionScopeContract Scope,
    PermissionActionContract Action,
    PermissionTypeContract Type,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? ExpiresAt = null,
    string? Note = null,
    DateTime? UpdatedAt = null);

/// <summary>
/// Область действия разрешения
/// </summary>
public enum PermissionScopeContract
{
    Tenant = 1,
    Project = 2,
    Task = 3
}

/// <summary>
/// Действие, на которое распространяется разрешение
/// </summary>
public enum PermissionActionContract
{
    ManageProjects = 1,
    ManageTasks = 2,
    ManageUsers = 3,
    ManageTenantSettings = 4,
    ViewReports = 5,
    ManagePermissions = 6
}

/// <summary>
/// Тип разрешения
/// </summary>
public enum PermissionTypeContract
{
    Read = 1,
    Create = 2,
    Update = 3,
    Delete = 4,
    FullAccess = 5
}

