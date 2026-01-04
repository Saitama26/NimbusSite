using Projects.Contracts.Enums;

namespace Projects.Contracts.Api.Requests;

/// <summary>
/// Запрос на изменение статуса проекта
/// </summary>
public sealed record ChangeProjectStatusRequest(
    ProjectStatusContract Status);

