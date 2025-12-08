using Application.Abstractions.Messaging;
using Domain.Tasks.ValueObjects;

namespace Application.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid TenantId,
    Guid ProjectId,
    string Title,
    string Description,
    Guid AssignedUserId,
    DateTime? DueDate = null,
    TaskPriority Priority = TaskPriority.Medium
) : ICommand<Guid> {}

