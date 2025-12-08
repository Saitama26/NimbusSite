using Application.Abstractions.Messaging;

namespace Application.Users.Commands.DeactivateUser;

public sealed record DeactivateUserCommand(
    Guid UserId
) : ICommand<Guid> {}

