using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Domain.Entities;
using Tasks.Infrastructure.Persistence.Configurations;
using Tasks.Infrastructure.Views.ProjectsViews;
using Tasks.Infrastructure.Views.TenantsViews;
using Tasks.Infrastructure.Views.UsersViews;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Infrastructure;

/// <summary>
/// DbContext для Tasks
/// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
/// </summary>
public class TasksDbContext : DbContext, ITasksDbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options)
    {
    }

    public DbSet<DomainTask> Tasks => Set<DomainTask>();
    public DbSet<UserView> UserViews => Set<UserView>();
    public DbSet<ProjectView> ProjectViews => Set<ProjectView>();
    public DbSet<TenantView> TenantViews => Set<TenantView>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectViewConfiguration());
        modelBuilder.ApplyConfiguration(new TenantViewConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}

