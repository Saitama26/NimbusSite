using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Projects.Infrastructure;

namespace Projects.Infrastructure.Repositories;

internal sealed class ProjectUserRepository : IProjectUserRepository
{
    private readonly ProjectsDbContext _dbContext;

    public ProjectUserRepository(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectUser?> GetByIdAsync(Guid projectUserId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == projectUserId, cancellationToken);
    }

    public async Task<ProjectUser?> GetByProjectAndUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId, cancellationToken);
    }

    public async Task<IQueryable<ProjectUser>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.ProjectUsers
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId);
    }

    public async Task<IQueryable<ProjectUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.ProjectUsers
            .AsNoTracking()
            .Where(x => x.UserId == userId);
    }

    public async Task<IQueryable<ProjectUser>> GetByUserIdAndTenantIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.ProjectUsers
            .AsNoTracking()
            .Join(
                _dbContext.Projects,
                pu => pu.ProjectId,
                p => p.Id,
                (pu, p) => new { ProjectUser = pu, Project = p })
            .Where(x => x.ProjectUser.UserId == userId && x.Project.TenantId == tenantId)
            .Select(x => x.ProjectUser);
    }

    public async Task<bool> ExistsAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectUsers
            .AsNoTracking()
            .AnyAsync(x => x.ProjectId == projectId && x.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(ProjectUser projectUser, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProjectUsers.AddAsync(projectUser, cancellationToken);
    }

    public Task UpdateAsync(ProjectUser projectUser, CancellationToken cancellationToken = default)
    {
        _dbContext.ProjectUsers.Update(projectUser);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProjectUser projectUser, CancellationToken cancellationToken = default)
    {
        _dbContext.ProjectUsers.Remove(projectUser);
        return Task.CompletedTask;
    }
}

