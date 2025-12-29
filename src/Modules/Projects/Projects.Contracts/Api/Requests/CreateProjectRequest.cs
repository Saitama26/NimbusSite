namespace Projects.Contracts.Api.Requests;

/// <summary>
/// Запрос на создание проекта
/// </summary>
public sealed record CreateProjectRequest(
    string Name,
    string? Description = null,
    string? TenantName = null);

