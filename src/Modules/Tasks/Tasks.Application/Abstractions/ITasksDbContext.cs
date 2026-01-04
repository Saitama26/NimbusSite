using Microsoft.EntityFrameworkCore;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Application.Abstractions;

/// <summary>
/// Интерфейс для доступа к DbContext без зависимости от Infrastructure
/// </summary>
public interface ITasksDbContext
{
    DbSet<DomainTask> Tasks { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

