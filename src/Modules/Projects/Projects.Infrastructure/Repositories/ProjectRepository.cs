using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;

namespace Projects.Infrastructure.Repositories;

internal sealed class ProjectRepository : IProjectRepository
{
    private readonly ProjectsDbContext _dbContext;

    public ProjectRepository(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<IQueryable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // возвращаем IQueryable для дальнейшей фильтрации
        await Task.CompletedTask;
        return _dbContext.Projects.AsNoTracking();
    }

    public async Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(p => p.TenantId == tenantId && p.Name == name, cancellationToken);
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(project);
        return Task.CompletedTask;
    }
}

