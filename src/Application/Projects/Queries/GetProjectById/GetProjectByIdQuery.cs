using Application.Abstractions.Messaging;

namespace Application.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid ProjectId) : IQuery<ProjectResponse> { }

