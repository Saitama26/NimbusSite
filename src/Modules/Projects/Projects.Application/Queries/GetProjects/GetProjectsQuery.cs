using Common.Application.Abstractions.Messaging;

namespace Projects.Application.Queries.GetProjects;

/// <summary>
/// Запрос получения списка проектов
/// </summary>
public sealed record GetProjectsQuery(int TenantId) : IQuery<IEnumerable<ProjectListItemDto>>;

/// <summary>
/// DTO для списка проектов (внутренний)
/// </summary>
public sealed record ProjectListItemDto(
    Guid Id,
    int TenantId,
    string Name,
    Projects.Domain.Enums.ProjectStatus Status,
    DateTime? UpdatedAt);

