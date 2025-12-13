using Tasks.Domain.Entities;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Application.Abstractions;

/// <summary>
/// Репозиторий задач (реализация в Infrastructure)
/// </summary>
public interface ITaskRepository
{
    Task<DomainTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    Task<IQueryable<DomainTask>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IQueryable<DomainTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IQueryable<DomainTask>> GetByAssignedUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTitleAsync(Guid tenantId, Guid projectId, string title, CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task AddAsync(DomainTask task, CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken cancellationToken = default);

    System.Threading.Tasks.Task DeleteAsync(DomainTask task, CancellationToken cancellationToken = default);
}

