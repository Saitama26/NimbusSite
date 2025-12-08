using Domain.Users.ValueObjects;

namespace Application.AccessPermissions.Queries.GetPermissions;

public sealed record PermissionResponse(
    Guid PermissionId,
    Guid TenantId,
    Guid UserId,
    Guid ProjectId,
    UserRole Role,
    DateTime CreatedAt,
    DateTime? RevokedAt) { }

