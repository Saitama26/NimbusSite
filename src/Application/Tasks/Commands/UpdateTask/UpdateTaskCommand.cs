using Application.Abstractions.Messaging;
using Domain.Tasks.ValueObjects;

namespace Application.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    string? Title = null,
    string? Description = null,
    DateTime? DueDate = null,
    TaskPriority? Priority = null
) : ICommand<Guid> {}

