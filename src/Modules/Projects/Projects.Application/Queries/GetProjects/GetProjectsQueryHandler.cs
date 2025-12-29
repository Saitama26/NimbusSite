using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Application.Queries.GetProjects;

namespace Projects.Application.Queries.GetProjects;

/// <summary>
/// Обработчик получения списка проектов
/// </summary>
internal sealed class GetProjectsQueryHandler : IQueryHandler<GetProjectsQuery, IEnumerable<ProjectListItemDto>>
{
    private readonly IProjectsDbContext _dbContext;

    public GetProjectsQueryHandler(IProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<ProjectListItemDto>>> Handle(GetProjectsQuery query, CancellationToken cancellationToken)
    {
        var projects = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.TenantId == query.TenantId && p.Status != Projects.Domain.Enums.ProjectStatus.Deleted)
            .Select(p => new ProjectListItemDto(
                p.Id,
                p.TenantId,
                p.Name,
                p.Status,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ProjectListItemDto>>.Success(projects);
    }
}

