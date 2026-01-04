using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions.Views;

namespace Tasks.Infrastructure.Views.ProjectsViews;

internal sealed class ProjectViewRepository : IProjectViewRepository
{
    private readonly TasksDbContext _dbContext;

    public ProjectViewRepository(TasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectViewDto?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectViews
            .AsNoTracking()
            .Where(x => x.Id == projectId)
            .Select(x => new ProjectViewDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectViewDto>> GetByIdsAsync(IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default)
    {
        var ids = projectIds?.ToList() ?? new List<Guid>();
        if (ids.Count == 0)
        {
            return Array.Empty<ProjectViewDto>();
        }

        return await _dbContext.ProjectViews
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new ProjectViewDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                Name = x.Name,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectViews
            .AsNoTracking()
            .AnyAsync(x => x.Id == projectId, cancellationToken);
    }
}

