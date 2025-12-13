using Microsoft.EntityFrameworkCore;
using AccessPermissions.Domain.Entities;
using AccessPermissions.Infrastructure.Persistence.Configurations;

namespace AccessPermissions.Infrastructure;

/// <summary>
/// DbContext для AccessPermissions
/// </summary>
public class AccessPermissionsDbContext : DbContext
{
    public AccessPermissionsDbContext(DbContextOptions<AccessPermissionsDbContext> options) : base(options)
    {
    }

    public DbSet<AccessPermission> AccessPermissions => Set<AccessPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessPermissionConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

