using Microsoft.EntityFrameworkCore;
using Tenants.Domain.Entities;

namespace Tenants.Application.Abstractions;

/// <summary>
/// Интерфейс для доступа к DbContext без зависимости от Infrastructure
/// </summary>
public interface ITenantsDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<UserTenant> UserTenants { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

