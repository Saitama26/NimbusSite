using Common.Application.Abstractions.Tenancy;
using Microsoft.EntityFrameworkCore;
using Tenants.Infrastructure.Persistence;

namespace Tenants.Infrastructure.Tenancy;

/// <summary>
/// Реализация ITenancyDomain для работы с тенантами
/// </summary>
internal sealed class TenancyDomain : ITenancyDomain
{
    private readonly TenantsDbContext _dbContext;

    public TenancyDomain(TenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> FindTenantIntAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == tenantName, cancellationToken);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant '{tenantName}' not found");
        }

        return tenant.TenantInt;
    }

    public async Task<TenantInfo> GetTenantInfoAsync(int tenantInt, CancellationToken cancellationToken = default)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantInt == tenantInt, cancellationToken);

        if (tenant == null)
        {
            throw new InvalidOperationException($"Tenant with TenantInt '{tenantInt}' not found");
        }

        return new TenantInfo
        {
            TenantInt = tenant.TenantInt,
            Name = tenant.Name,
            ConnectionString = tenant.ConnectionString,
            CreatedAt = tenant.CreatedAt
        };
    }
}

