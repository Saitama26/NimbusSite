using Application.Abstractions.Messaging;

namespace Application.AccessPermissions.Queries.GetPermissions;

public sealed record GetPermissionsQuery(
    Guid? UserId = null,
    Guid? ProjectId = null,
    Guid? TenantId = null,
    bool IncludeRevoked = false
) : IQuery<IReadOnlyList<PermissionResponse>> { }

