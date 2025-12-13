using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Entities;
using Tasks.Infrastructure.Persistence.Configurations;
using Tasks.Infrastructure.Persistence.Sharding;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TaskConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

