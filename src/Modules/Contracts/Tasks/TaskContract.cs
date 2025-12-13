namespace Contracts.Tasks;

/// <summary>
/// Контракт для обмена данными о задаче между модулями
/// </summary>
public sealed record TaskContract(
    Guid Id,
    Guid TenantId,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskStatusContract Status,
    TaskPriorityContract Priority,
    Guid CreatedByUserId,
    Guid? AssignedToUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null,
    DateTime? DueDate = null,
    DateTime? StartedAt = null,
    DateTime? CompletedAt = null);

/// <summary>
/// Статус задачи
/// </summary>
public enum TaskStatusContract
{
    New = 1,
    InProgress = 2,
    InReview = 3,
    Completed = 4,
    Cancelled = 5,
    Blocked = 6,
    Deleted = 7
}

/// <summary>
/// Приоритет задачи
/// </summary>
public enum TaskPriorityContract
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}

