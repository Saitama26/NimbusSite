using Application.Abstractions.Repositories;
using Application.Abstractions.Messaging;
using Domain.Projects;
using SharedKernel;

namespace Application.Projects.Queries.GetProjects;

internal sealed class GetProjectsQueryHandler : IQueryHandler<GetProjectsQuery, IReadOnlyList<ProjectResponse>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<IReadOnlyList<ProjectResponse>>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<Project> projects;

        if (query.TenantId.HasValue)
        {
            projects = await _projectRepository.GetByTenantIdAsync(query.TenantId.Value, cancellationToken);
        }
        else if (query.OwnerId.HasValue)
        {
            projects = await _projectRepository.GetByManagerIdAsync(query.OwnerId.Value, cancellationToken);
        }
        else
        {
            projects = await _projectRepository.GetAllAsync(cancellationToken);
        }

        var response = projects.Select(project => new ProjectResponse(
            project.Id,
            project.TenantId,
            project.Name,
            project.Description,
            project.OwnerId,
            project.StartDate,
            project.EndDate,
            project.Status,
            project.CreatedAt
        )).ToList();

        return response;
    }
}

