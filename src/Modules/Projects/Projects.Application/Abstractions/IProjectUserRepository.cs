using Projects.Domain.Entities;

namespace Projects.Application.Abstractions;

/// <summary>
/// Репозиторий для работы со связями Project-User
/// </summary>
public interface IProjectUserRepository
{
    /// <summary>
    /// Получить связь по ID
    /// </summary>
    Task<ProjectUser?> GetByIdAsync(Guid projectUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить связь по ProjectId и UserId
    /// </summary>
    Task<ProjectUser?> GetByProjectAndUserAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все связи для проекта
    /// </summary>
    Task<IQueryable<ProjectUser>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все связи для пользователя
    /// </summary>
    Task<IQueryable<ProjectUser>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все связи для пользователя в рамках тенанта
    /// </summary>
    Task<IQueryable<ProjectUser>> GetByUserIdAndTenantIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование связи
    /// </summary>
    Task<bool> ExistsAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую связь
    /// </summary>
    Task AddAsync(ProjectUser projectUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить связь
    /// </summary>
    Task UpdateAsync(ProjectUser projectUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить связь
    /// </summary>
    Task DeleteAsync(ProjectUser projectUser, CancellationToken cancellationToken = default);
}

