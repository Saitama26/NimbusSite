using Domain.Projects;

namespace Application.Abstractions.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
    void Add(Project project);
    void Remove(Project project);
}

