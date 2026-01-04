using Common.Application.Abstractions.Messaging;

namespace Tasks.Application.Queries.GetTaskById;

/// <summary>
/// Запрос получения задачи по ID
/// </summary>
public sealed record GetTaskByIdQuery(
    Guid TaskId,
    int TenantId) : IQuery<TaskDto>;

/// <summary>
/// DTO для задачи (внутренний)
/// </summary>
public sealed record TaskDto(
    Guid Id,
    int TenantId,
    Guid ProjectId,
    string Title,
    string? Description,
    Tasks.Domain.Enums.TaskStatus Status,
    Tasks.Domain.Enums.TaskPriority Priority,
    Guid? AssignedToUserId,
    Guid CreatedByUserId,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);

