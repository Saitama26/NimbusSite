using Microsoft.EntityFrameworkCore;
using Tenants.Domain.Entities;
using Tenants.Infrastructure.Persistence.Configurations;
using Tenants.Infrastructure.Persistence.Sharding;

namespace Tenants.Infrastructure.Persistence;

/// <summary>
/// DbContext каталога тенантов и карты шардов.
/// </summary>
public class TenantsDbContext : DbContext
{
    public TenantsDbContext(DbContextOptions<TenantsDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ShardMapEntry> ShardMapEntries => Set<ShardMapEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

