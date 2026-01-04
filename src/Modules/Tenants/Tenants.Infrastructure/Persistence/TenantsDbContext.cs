using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Domain.Entities;
using Tenants.Infrastructure.Persistence.Configurations;

namespace Tenants.Infrastructure.Persistence;

/// <summary>
/// DbContext для работы с центральной БД тенантов (NimbusSite_Tenants)
/// Таблицы находятся в корне базы данных без схем
/// </summary>
public class TenantsDbContext : DbContext, ITenantsDbContext
{
    public TenantsDbContext(DbContextOptions<TenantsDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new UserTenantConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

