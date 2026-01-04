using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Api.Requests;

/// <summary>
/// Запрос на изменение статуса задачи
/// </summary>
public sealed record ChangeTaskStatusRequest(
    TaskStatusContract Status);

