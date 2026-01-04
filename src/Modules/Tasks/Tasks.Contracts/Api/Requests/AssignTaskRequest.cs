namespace Tasks.Contracts.Api.Requests;

/// <summary>
/// Запрос на назначение задачи пользователю
/// </summary>
public sealed record AssignTaskRequest(
    Guid? AssignedToUserId);

