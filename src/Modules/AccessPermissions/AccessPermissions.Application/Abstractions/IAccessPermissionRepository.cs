using AccessPermissions.Domain.Entities;
using AccessPermissions.Domain.Enums;

namespace AccessPermissions.Application.Abstractions;

/// <summary>
/// Репозиторий разрешений доступа
/// </summary>
public interface IAccessPermissionRepository
{
    /// <summary>
    /// Получить разрешение по ID
    /// </summary>
    Task<AccessPermission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все разрешения (IQueryable для фильтрации)
    /// </summary>
    Task<IQueryable<AccessPermission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить разрешения пользователя в тенанте
    /// </summary>
    Task<IQueryable<AccessPermission>> GetByUserIdAndTenantIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить разрешения пользователя в проекте
    /// </summary>
    Task<IQueryable<AccessPermission>> GetByUserIdAndProjectIdAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить разрешения для проекта
    /// </summary>
    Task<IQueryable<AccessPermission>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить разрешения для задачи
    /// </summary>
    Task<IQueryable<AccessPermission>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование разрешения
    /// </summary>
    Task<bool> ExistsAsync(
        Guid tenantId,
        Guid userId,
        PermissionScope scope,
        PermissionAction action,
        PermissionType type,
        Guid? projectId = null,
        Guid? taskId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, имеет ли пользователь разрешение
    /// </summary>
    Task<bool> HasPermissionAsync(
        Guid userId,
        Guid tenantId,
        PermissionAction action,
        PermissionType type,
        Guid? projectId = null,
        Guid? taskId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить разрешение
    /// </summary>
    Task AddAsync(AccessPermission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить разрешение
    /// </summary>
    Task UpdateAsync(AccessPermission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить разрешение
    /// </summary>
    Task DeleteAsync(AccessPermission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Массовое удаление по пользователю
    /// </summary>
    Task<int> DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Массовое удаление по проекту
    /// </summary>
    Task<int> DeleteByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Массовое удаление по задаче
    /// </summary>
    Task<int> DeleteByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Массовое удаление по тенанту
    /// </summary>
    Task<int> DeleteByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

