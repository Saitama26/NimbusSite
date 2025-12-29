using Common.Application.Abstractions.Messaging;

namespace Projects.Application.Queries.GetProjectById;

/// <summary>
/// Запрос получения проекта по ID
/// </summary>
public sealed record GetProjectByIdQuery(
    Guid ProjectId,
    int TenantId) : IQuery<ProjectDto>;

/// <summary>
/// DTO для проекта (внутренний)
/// </summary>
public sealed record ProjectDto(
    Guid Id,
    int TenantId,
    string Name,
    string? Description,
    Projects.Domain.Enums.ProjectStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

