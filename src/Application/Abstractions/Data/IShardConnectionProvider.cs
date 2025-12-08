namespace Application.Abstractions.Data;

/// <summary>
/// Провайдер для получения connection string для шарда
/// </summary>
public interface IShardConnectionProvider
{
    /// <summary>
    /// Получить connection string для указанного тенанта
    /// </summary>
    Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить connection string для текущего тенанта из контекста
    /// </summary>
    Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default);
}

