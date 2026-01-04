using Microsoft.EntityFrameworkCore;
using Projects.Domain.Entities;

namespace Projects.Application.Abstractions;

/// <summary>
/// Интерфейс для доступа к DbContext без зависимости от Infrastructure
/// </summary>
public interface IProjectsDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<ProjectUser> ProjectUsers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

