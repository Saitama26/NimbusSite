using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Api.Responses;

/// <summary>
/// Ответ с информацией о задаче
/// </summary>
public sealed record TaskResponse(
    Guid Id,
    int TenantId,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskStatusContract Status,
    TaskPriorityContract Priority,
    Guid? AssignedToUserId,
    Guid CreatedByUserId,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);

