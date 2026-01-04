using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление задачи
/// </summary>
public sealed record UpdateTaskRequest(
    string? Title = null,
    string? Description = null,
    TaskPriorityContract? Priority = null,
    DateTime? DueDate = null);

