using Application.Abstractions.Messaging;
using Domain.Users.ValueObjects;

namespace Application.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleCommand(
    Guid UserId,
    UserRole NewRole
) : ICommand<Guid> {}

