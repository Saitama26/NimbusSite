using Tenants.Domain.Entities;
using Tenants.Domain.Enums;

namespace Tenants.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для работы с тенантами
/// Реализация будет в Infrastructure слое
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Получить тенанта по ID
    /// </summary>
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить тенанта по поддомену
    /// </summary>
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить IQueryable всех тенантов для OData пагинации, фильтрации и сортировки
    /// </summary>
    Task<IQueryable<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование тенанта с указанным поддоменом
    /// </summary>
    Task<bool> ExistsBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить нового тенанта
    /// </summary>
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить тенанта
    /// </summary>
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить тенанта (hard delete, если нужно)
    /// </summary>
    Task DeleteAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

