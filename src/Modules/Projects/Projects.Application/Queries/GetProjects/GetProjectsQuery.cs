using Common.Application.Abstractions.Messaging;
using Projects.Application.DTOs;

namespace Projects.Application.Queries.GetProjects;

/// <summary>
/// Запрос получения списка проектов (IQueryable для OData-подобной фильтрации).
/// </summary>
public sealed record GetProjectsQuery() : IQuery<IQueryable<ProjectListItemDto>>;

