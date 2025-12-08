using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Domain.Entities;

namespace Tenants.Infrastructure.Repositories;

internal sealed class TenantRepository : ITenantRepository
{
    private readonly Persistence.TenantsDbContext _dbContext;

    public TenantRepository(Persistence.TenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
    }

    public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var normalized = subdomain.ToLowerInvariant().Trim();
        return await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Subdomain == normalized, cancellationToken);
    }

    public Task<IQueryable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // IQueryable остается валидным в рамках жизненного цикла DbContext (scoped).
        return Task.FromResult(_dbContext.Tenants.AsNoTracking().AsQueryable());
    }

    public async Task<bool> ExistsBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var normalized = subdomain.ToLowerInvariant().Trim();
        return await _dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(x => x.Subdomain == normalized, cancellationToken);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }

    public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _dbContext.Tenants.Update(tenant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        _dbContext.Tenants.Remove(tenant);
        return Task.CompletedTask;
    }
}

