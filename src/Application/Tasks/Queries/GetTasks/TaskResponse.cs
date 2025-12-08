using Domain.Tasks.ValueObjects;

namespace Application.Tasks.Queries.GetTasks;

public sealed record TaskResponse(
    Guid TaskId,
    Guid TenantId,
    Guid ProjectId,
    string Title,
    string Description,
    Guid AssignedUserId,
    DateTime? DueDate,
    Status Status,
    TaskPriority Priority,
    DateTime CreatedAt) { }

