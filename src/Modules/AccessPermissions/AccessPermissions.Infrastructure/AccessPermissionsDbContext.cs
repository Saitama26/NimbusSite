using Microsoft.EntityFrameworkCore;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Domain.Entities;
using AccessPermissions.Infrastructure.Persistence.Configurations;
using AccessPermissions.Infrastructure.Views.ProjectsViews;
using AccessPermissions.Infrastructure.Views.TasksViews;
using AccessPermissions.Infrastructure.Views.TenantsViews;
using AccessPermissions.Infrastructure.Views.UsersViews;

namespace AccessPermissions.Infrastructure;

/// <summary>
/// DbContext для AccessPermissions
/// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
/// </summary>
public class AccessPermissionsDbContext : DbContext, IAccessPermissionsDbContext
{
    public AccessPermissionsDbContext(DbContextOptions<AccessPermissionsDbContext> options) : base(options)
    {
    }

    public DbSet<AccessPermission> AccessPermissions => Set<AccessPermission>();
    public DbSet<UserView> UserViews => Set<UserView>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<TaskView> TaskViews => Set<TaskView>();
    public DbSet<TenantView> TenantViews => Set<TenantView>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new AccessPermissionConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectViewConfiguration());
        modelBuilder.ApplyConfiguration(new TaskViewConfiguration());
        modelBuilder.ApplyConfiguration(new TenantViewConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}

