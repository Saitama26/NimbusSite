namespace Identity.Application.Abstractions;

/// <summary>
/// Резолвер connection string по TenantId для шардирования
/// </summary>
public interface IShardResolver
{
    /// <summary>
    /// Получить connection string для указанного тенанта
    /// </summary>
    Task<string?> ResolveConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

