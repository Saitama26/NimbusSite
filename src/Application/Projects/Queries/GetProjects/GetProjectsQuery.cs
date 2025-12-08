using Application.Abstractions.Messaging;

namespace Application.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(
    Guid? TenantId = null,
    Guid? OwnerId = null
) : IQuery<IReadOnlyList<ProjectResponse>> { }

