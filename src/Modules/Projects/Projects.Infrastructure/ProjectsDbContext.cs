using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Domain.Entities;
using Projects.Infrastructure.Persistence.Configurations;
using Projects.Infrastructure.Views.UsersViews;

namespace Projects.Infrastructure;

/// <summary>
/// DbContext для Projects
/// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
/// </summary>
public class ProjectsDbContext : DbContext, IProjectsDbContext
{
    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectUser> ProjectUsers => Set<ProjectUser>();
    public DbSet<UserView> UserViews => Set<UserView>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectUserConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}

