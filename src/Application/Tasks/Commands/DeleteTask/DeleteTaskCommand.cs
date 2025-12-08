using Application.Abstractions.Messaging;

namespace Application.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(
    Guid TaskId
) : ICommand<Guid> {}

