namespace Application.Abstractions.Data;

/// <summary>
/// Интерфейс для выбора шарда базы данных на основе TenantId
/// </summary>
public interface IShardSelector
{
    /// <summary>
    /// Получить connection string для указанного TenantId
    /// </summary>
    Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить имя шарда для указанного TenantId
    /// </summary>
    Task<string> GetShardNameAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, существует ли шард для указанного TenantId
    /// </summary>
    Task<bool> ShardExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

