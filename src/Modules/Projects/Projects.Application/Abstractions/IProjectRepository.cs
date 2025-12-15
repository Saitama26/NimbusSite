using Projects.Domain.Entities;

namespace Projects.Application.Abstractions;

/// <summary>
/// Репозиторий проектов (реализация в Infrastructure).
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<IQueryable<Project>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);

    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    Task DeleteAsync(Project project, CancellationToken cancellationToken = default);

    Task<IQueryable<Project>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

