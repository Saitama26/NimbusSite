using Application.Abstractions.Messaging;

namespace Application.AccessPermissions.Commands.RevokePermission;

public sealed record RevokePermissionCommand(
    Guid UserId,
    Guid ProjectId
) : ICommand<Guid> {}

