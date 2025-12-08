using Domain.Tasks;

namespace Application.Abstractions.Repositories;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTask>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTask>> GetByAssigneeIdAsync(Guid assigneeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTask>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectTask>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default);
    void Add(ProjectTask task);
    void Remove(ProjectTask task);
}

