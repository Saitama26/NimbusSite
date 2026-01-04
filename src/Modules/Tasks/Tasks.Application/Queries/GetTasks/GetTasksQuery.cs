using Common.Application.Abstractions.Messaging;

namespace Tasks.Application.Queries.GetTasks;

/// <summary>
/// Запрос получения списка задач
/// </summary>
public sealed record GetTasksQuery(int TenantId) : IQuery<IEnumerable<TaskListItemDto>>;

/// <summary>
/// DTO для списка задач (внутренний)
/// </summary>
public sealed record TaskListItemDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    Tasks.Domain.Enums.TaskStatus Status,
    Tasks.Domain.Enums.TaskPriority Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    DateTime CreatedAt);

