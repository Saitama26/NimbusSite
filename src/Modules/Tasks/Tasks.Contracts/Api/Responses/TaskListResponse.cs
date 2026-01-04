using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Api.Responses;

/// <summary>
/// Ответ со списком задач
/// </summary>
public sealed record TaskListResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    TaskStatusContract Status,
    TaskPriorityContract Priority,
    Guid? AssignedToUserId,
    DateTime? DueDate,
    DateTime CreatedAt);

