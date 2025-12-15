using Microsoft.EntityFrameworkCore;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence.Configurations;
using Projects.Infrastructure.Persistence.Sharding;
using Projects.Infrastructure.Views.UsersViews;

namespace Projects.Infrastructure;

/// <summary>
/// DbContext для Projects.
/// </summary>
public class ProjectsDbContext : DbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<ShardMapEntry> ShardMapEntries => Set<ShardMapEntry>();
    public DbSet<UserView> UserViews => Set<UserView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectUserConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

