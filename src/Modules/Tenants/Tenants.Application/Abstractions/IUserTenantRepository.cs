using Tenants.Domain.Entities;
using Users.Domain.Enums;

namespace Tenants.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для работы со связями User-Tenant
/// Реализация будет в Infrastructure слое
/// </summary>
public interface IUserTenantRepository
{
    /// <summary>
    /// Получить связь UserTenant по ID
    /// </summary>
    Task<UserTenant?> GetByIdAsync(Guid userTenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить связь UserTenant по UserId и TenantId
    /// </summary>
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все связи пользователя с тенантами
    /// </summary>
    Task<IQueryable<UserTenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все связи тенанта с пользователями
    /// </summary>
    Task<IQueryable<UserTenant>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить тенант, которым владеет пользователь (IsOwner=true)
    /// </summary>
    Task<UserTenant?> GetOwnerTenantByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, является ли пользователь владельцем тенанта
    /// </summary>
    Task<bool> IsUserOwnerOfTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, существует ли связь между пользователем и тенантом
    /// </summary>
    Task<bool> ExistsAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, есть ли у пользователя тенант, которым он владеет
    /// </summary>
    Task<bool> HasOwnerTenantAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую связь UserTenant
    /// </summary>
    Task AddAsync(UserTenant userTenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить связь UserTenant
    /// </summary>
    Task UpdateAsync(UserTenant userTenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить связь UserTenant
    /// </summary>
    Task DeleteAsync(UserTenant userTenant, CancellationToken cancellationToken = default);
}

