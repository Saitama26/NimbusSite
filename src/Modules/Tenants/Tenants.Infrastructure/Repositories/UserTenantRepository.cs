using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Domain.Entities;

namespace Tenants.Infrastructure.Repositories;

internal sealed class UserTenantRepository : IUserTenantRepository
{
    private readonly Persistence.TenantsDbContext _dbContext;

    public UserTenantRepository(Persistence.TenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserTenant?> GetByIdAsync(Guid userTenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userTenantId, cancellationToken);
    }

    public async Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId, cancellationToken);
    }

    public Task<IQueryable<UserTenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.UserTenants
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AsQueryable());
    }

    public Task<IQueryable<UserTenant>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.UserTenants
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable());
    }

    public async Task<UserTenant?> GetOwnerTenantByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsOwner == true, cancellationToken);
    }

    public async Task<bool> IsUserOwnerOfTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.TenantId == tenantId && x.IsOwner == true, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> HasOwnerTenantAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserTenants
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.IsOwner == true, cancellationToken);
    }

    public async Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserTenants.AddAsync(userTenant, cancellationToken);
    }

    public Task UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        _dbContext.UserTenants.Update(userTenant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserTenant userTenant, CancellationToken cancellationToken = default)
    {
        _dbContext.UserTenants.Remove(userTenant);
        return Task.CompletedTask;
    }
}

