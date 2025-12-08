using SharedKernel;

namespace Domain.Access.Errors;

public static class AccessPermissionErrors
{
    public static Error NotFound(Guid permissionId) =>
        new Error("AccessPermission.NotFound",
            $"The access permission with the id - {permissionId} was not found",
            ErrorType.NotFound);

    public static Error AlreadyGranted(Guid userId, Guid projectId) =>
        new Error("AccessPermission.AlreadyGranted",
            $"The permission for user {userId} on project {projectId} is already granted",
            ErrorType.Conflict);

    public static Error AlreadyRevoked(Guid userId, Guid projectId) =>
        new Error("AccessPermission.AlreadyRevoked",
            $"The permission for user {userId} on project {projectId} is already revoked",
            ErrorType.Failure);

    public static Error NotGranted(Guid userId, Guid projectId) =>
        new Error("AccessPermission.NotGranted",
            $"The permission for user {userId} on project {projectId} is not granted",
            ErrorType.NotFound);
}

