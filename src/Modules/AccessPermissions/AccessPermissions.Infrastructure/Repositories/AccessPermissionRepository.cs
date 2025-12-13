using Microsoft.EntityFrameworkCore;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using AccessPermissions.Domain.Enums;
using AccessPermissions.Infrastructure;

namespace AccessPermissions.Infrastructure.Repositories;

internal sealed class AccessPermissionRepository : IAccessPermissionRepository
{
    private readonly AccessPermissionsDbContext _dbContext;

    public AccessPermissionRepository(AccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccessPermission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessPermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == permissionId, cancellationToken);
    }

    public async Task<IQueryable<AccessPermission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.AccessPermissions.AsNoTracking();
    }

    public async Task<IQueryable<AccessPermission>> GetByUserIdAndTenantIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.TenantId == tenantId);
    }

    public async Task<IQueryable<AccessPermission>> GetByUserIdAndProjectIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.ProjectId == projectId);
    }

    public async Task<IQueryable<AccessPermission>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.ProjectId == projectId);
    }

    public async Task<IQueryable<AccessPermission>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.TaskId == taskId);
    }

    public async Task<bool> ExistsAsync(
        Guid tenantId,
        Guid userId,
        PermissionScope scope,
        PermissionAction action,
        PermissionType type,
        Guid? projectId = null,
        Guid? taskId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId
                && p.UserId == userId
                && p.Scope == scope
                && p.Action == action
                && p.Type == type);

        if (projectId.HasValue)
        {
            query = query.Where(p => p.ProjectId == projectId);
        }
        else
        {
            query = query.Where(p => p.ProjectId == null);
        }

        if (taskId.HasValue)
        {
            query = query.Where(p => p.TaskId == taskId);
        }
        else
        {
            query = query.Where(p => p.TaskId == null);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        PermissionAction action,
        PermissionType type,
        Guid? projectId = null,
        Guid? taskId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.UserId == userId
                && p.TenantId == tenantId
                && p.Action == action
                && (p.Type == type || p.Type == PermissionType.FullAccess)
                && p.IsValid);

        // Проверяем разрешения на уровне тенанта
        var tenantPermissions = query.Where(p => p.Scope == PermissionScope.Tenant && p.ProjectId == null);

        if (await tenantPermissions.AnyAsync(cancellationToken))
        {
            return true;
        }

        // Проверяем разрешения на уровне проекта
        if (projectId.HasValue)
        {
            var projectPermissions = query.Where(p => p.Scope == PermissionScope.Project && p.ProjectId == projectId);
            if (await projectPermissions.AnyAsync(cancellationToken))
            {
                return true;
            }
        }

        // Проверяем разрешения на уровне задачи
        if (taskId.HasValue)
        {
            var taskPermissions = query.Where(p => p.Scope == PermissionScope.Task && p.TaskId == taskId);
            if (await taskPermissions.AnyAsync(cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    public async Task AddAsync(AccessPermission permission, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccessPermissions.AddAsync(permission, cancellationToken);
    }

    public Task UpdateAsync(AccessPermission permission, CancellationToken cancellationToken = default)
    {
        _dbContext.AccessPermissions.Update(permission);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AccessPermission permission, CancellationToken cancellationToken = default)
    {
        _dbContext.AccessPermissions.Remove(permission);
        return Task.CompletedTask;
    }
}

