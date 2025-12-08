using Application.Abstractions.Data;
using Application.Abstractions.Repositories;
using Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TaskRepository : ITaskRepository
{
    private readonly IApplicationDbContext _context;

    public TaskRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.AssignedUserId == assigneeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .AnyAsync(t => t.Id == taskId, cancellationToken);
    }

    public void Add(ProjectTask task)
    {
        _context.Tasks.Add(task);
    }

    public void Remove(ProjectTask task)
    {
        _context.Tasks.Remove(task);
    }

    public async Task<IReadOnlyList<ProjectTask>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .ToListAsync(cancellationToken);
    }
}