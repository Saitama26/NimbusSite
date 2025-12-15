using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Entities;
using Tasks.Infrastructure.Persistence.Configurations;
using Tasks.Infrastructure.Persistence.Sharding;
using Tasks.Infrastructure.Views.ProjectsViews;
using Tasks.Infrastructure.Views.TenantsViews;
using Tasks.Infrastructure.Views.UsersViews;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Infrastructure;

/// <summary>
/// DbContext для Tasks
/// </summary>
public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<DomainTask> Tasks => Set<DomainTask>();
    public DbSet<ShardMapEntry> ShardMapEntries => Set<ShardMapEntry>();
    public DbSet<UserView> UserViews => Set<UserView>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<TenantView> TenantViews => Set<TenantView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectViewConfiguration());
        modelBuilder.ApplyConfiguration(new TenantViewConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

