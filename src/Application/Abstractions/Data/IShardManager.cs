namespace Application.Abstractions.Data;

/// <summary>
/// Интерфейс для управления шардами базы данных
/// </summary>
public interface IShardManager
{
    /// <summary>
    /// Создать новый шард для TenantId
    /// </summary>
    Task CreateShardAsync(Guid tenantId, string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить шард для TenantId
    /// </summary>
    Task DeleteShardAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список всех шардов
    /// </summary>
    Task<IReadOnlyList<ShardInfo>> GetAllShardsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить connection string для шарда
    /// </summary>
    Task UpdateShardConnectionStringAsync(Guid tenantId, string connectionString, CancellationToken cancellationToken = default);
}

/// <summary>
/// Информация о шарде
/// </summary>
public record ShardInfo(
    Guid TenantId,
    string ShardName,
    string ConnectionString,
    DateTime CreatedAt,
    bool IsActive);

