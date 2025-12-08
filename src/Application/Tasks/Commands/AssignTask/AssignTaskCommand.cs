using Application.Abstractions.Messaging;

namespace Application.Tasks.Commands.AssignTask;

public sealed record AssignTaskCommand(
    Guid TaskId,
    Guid AssignedUserId
) : ICommand<Guid> {}

