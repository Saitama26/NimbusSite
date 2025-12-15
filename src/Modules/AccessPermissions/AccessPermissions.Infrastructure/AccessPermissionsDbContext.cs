using Microsoft.EntityFrameworkCore;
using AccessPermissions.Domain.Entities;
using AccessPermissions.Infrastructure.Persistence.Configurations;
using AccessPermissions.Infrastructure.Views.ProjectsViews;
using AccessPermissions.Infrastructure.Views.TasksViews;
using AccessPermissions.Infrastructure.Views.TenantsViews;
using AccessPermissions.Infrastructure.Views.UsersViews;

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
    public DbSet<UserView> UserViews => Set<UserView>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<TaskView> TaskViews => Set<TaskView>();
    public DbSet<TenantView> TenantViews => Set<TenantView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccessPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectViewConfiguration());
        modelBuilder.ApplyConfiguration(new TaskViewConfiguration());
        modelBuilder.ApplyConfiguration(new TenantViewConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

