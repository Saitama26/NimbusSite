using Application.Abstractions.Messaging;
using Domain.Users.ValueObjects;

namespace Application.AccessPermissions.Commands.GrantPermission;

public sealed record GrantPermissionCommand(
    Guid TenantId,
    Guid UserId,
    Guid ProjectId,
    UserRole Role
) : ICommand<Guid> {}

