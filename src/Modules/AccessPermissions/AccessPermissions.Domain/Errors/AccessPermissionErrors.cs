using Common.Domain.Results;

namespace AccessPermissions.Domain.Errors;

/// <summary>
/// Ошибки домена AccessPermissions
/// </summary>
public static class AccessPermissionErrors
{
    public static Error NotFound(Guid permissionId) =>
        Error.NotFound("AccessPermission.NotFound", $"Access permission with ID {permissionId} was not found.");

    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("AccessPermission.UserNotFound", $"User with ID {userId} was not found.");

    public static Error ProjectNotFound(Guid projectId) =>
        Error.NotFound("AccessPermission.ProjectNotFound", $"Project with ID {projectId} was not found.");

    public static Error TaskNotFound(Guid taskId) =>
        Error.NotFound("AccessPermission.TaskNotFound", $"Task with ID {taskId} was not found.");

    public static Error TenantNotFound(Guid tenantId) =>
        Error.NotFound("AccessPermission.TenantNotFound", $"Tenant with ID {tenantId} was not found.");

    public static Error PermissionAlreadyExists =>
        Error.Conflict("AccessPermission.AlreadyExists", "Permission already exists for this user, scope, action, and type.");

    public static Error InvalidScope =>
        Error.Validation("AccessPermission.InvalidScope", "Invalid permission scope. ProjectId or TaskId must be set based on scope.");

    public static Error PermissionExpired =>
        Error.Validation("AccessPermission.Expired", "Permission has expired.");

    public static Error CannotModifyOwnPermission =>
        Error.Validation("AccessPermission.CannotModifyOwn", "Cannot modify your own permission.");
}

