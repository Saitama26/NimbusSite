using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Application.Queries.GetProjectById;
using Projects.Domain.Errors;

namespace Projects.Application.Queries.GetProjectById;

/// <summary>
/// Обработчик получения проекта по ID
/// </summary>
internal sealed class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectsDbContext _dbContext;

    public GetProjectByIdQueryHandler(IProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.ProjectId && p.TenantId == query.TenantId && p.Status != Projects.Domain.Enums.ProjectStatus.Deleted, cancellationToken);
        if (project == null)
        {
            return Result<ProjectDto>.Failure(ProjectErrors.NotFound(query.ProjectId));
        }

        var dto = new ProjectDto(
            project.Id,
            project.TenantId,
            project.Name,
            project.Description,
            project.Status,
            project.CreatedAt,
            project.UpdatedAt);

        return Result<ProjectDto>.Success(dto);
    }
}

