namespace Tasks.Contracts.Api.Responses;

/// <summary>
/// Ответ при создании задачи
/// </summary>
public sealed record CreateTaskResponse(
    Guid TaskId);

