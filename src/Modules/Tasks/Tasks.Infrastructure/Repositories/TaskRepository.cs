using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Domain.Entities;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Infrastructure.Repositories;

internal sealed class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _dbContext;

    public TaskRepository(TasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DomainTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    public async Task<IQueryable<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await System.Threading.Tasks.Task.CompletedTask;
        return _dbContext.Tasks.AsNoTracking();
    }

    public async Task<IQueryable<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await System.Threading.Tasks.Task.CompletedTask;
        return _dbContext.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId);
    }

    public async Task<IQueryable<DomainTask>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await System.Threading.Tasks.Task.CompletedTask;
        return _dbContext.Tasks.AsNoTracking().Where(t => t.AssignedToUserId == userId);
    }

    public async Task<bool> ExistsByTitleAsync(Guid tenantId, Guid projectId, string title, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .AnyAsync(t => t.TenantId == tenantId && t.ProjectId == projectId && t.Title == title, cancellationToken);
    }

    public System.Threading.Tasks.Task AddAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tasks.AddAsync(task, cancellationToken).AsTask();
    }

    public System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task DeleteAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Remove(task);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}

