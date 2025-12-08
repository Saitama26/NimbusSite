using Application.Abstractions.Data;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class ProjectRepository : IProjectRepository
{
    private readonly IApplicationDbContext _context;

    public ProjectRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == managerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(p => p.TenantId == tenantId && p.Name == name, cancellationToken);
    }

    public void Add(Project project)
    {
        _context.Projects.Add(project);
    }

    public void Remove(Project project)
    {
        _context.Projects.Remove(project);
    }
}