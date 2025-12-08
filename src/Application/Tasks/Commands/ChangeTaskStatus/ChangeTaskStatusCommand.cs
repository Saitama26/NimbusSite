using Application.Abstractions.Messaging;
using Domain.Tasks.ValueObjects;

namespace Application.Tasks.Commands.ChangeTaskStatus;

public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    Status NewStatus
) : ICommand<Guid> {}

