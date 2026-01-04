using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Api.Requests;

/// <summary>
/// Запрос на создание задачи
/// </summary>
public sealed record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    TaskPriorityContract Priority = TaskPriorityContract.Normal,
    string? Description = null,
    Guid? AssignedToUserId = null,
    DateTime? DueDate = null);

