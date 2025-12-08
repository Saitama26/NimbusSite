using Application.Abstractions.Repositories;
using Application.Abstractions.Messaging;
using Domain.Projects.Errors;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Projects.Queries.GetProjectById;

internal sealed class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly Application.Abstractions.Data.IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(
        IProjectRepository projectRepository,
        Application.Abstractions.Data.IApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _context = context;
    }

    public async Task<Result<ProjectResponse>> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId, cancellationToken);

        if (project is null)
        {
            return ProjectErrors.NotFound(query.ProjectId);
        }

        var memberIds = project.Members?.Select(m => m.Id).ToList() ?? new List<Guid>();

        return new ProjectResponse(
            project.Id,
            project.TenantId,
            project.Name,
            project.Description,
            project.OwnerId,
            project.StartDate,
            project.EndDate,
            project.Status,
            project.CreatedAt,
            memberIds);
    }
}

