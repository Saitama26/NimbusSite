using Common.Application.Abstractions.Messaging;
using Projects.Application.DTOs;

namespace Projects.Application.Queries.GetProjectById;

/// <summary>
/// Запрос получения проекта по ID.
/// </summary>
public sealed record GetProjectByIdQuery(Guid ProjectId) : IQuery<ProjectDto>;

