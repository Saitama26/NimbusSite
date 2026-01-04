using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccessPermissions.Application.Abstractions.Views;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Infrastructure.Views.TasksViews;

internal sealed class TaskViewRepository : ITaskViewRepository
{
    private readonly AccessPermissionsDbContext _dbContext;

    public TaskViewRepository(AccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskViewDto?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskViews
            .AsNoTracking()
            .Where(x => x.Id == taskId)
            .Select(x => new TaskViewDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                TenantId = x.TenantId,
                Title = x.Title,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskViewDto>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default)
    {
        var ids = taskIds?.ToList() ?? new List<Guid>();
        if (ids.Count == 0)
        {
            return Array.Empty<TaskViewDto>();
        }

        return await _dbContext.TaskViews
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new TaskViewDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                TenantId = x.TenantId,
                Title = x.Title,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaskViews
            .AsNoTracking()
            .AnyAsync(x => x.Id == taskId, cancellationToken);
    }
}

