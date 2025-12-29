namespace Projects.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление проекта
/// </summary>
public sealed record UpdateProjectRequest(
    string? Name = null,
    string? Description = null);

