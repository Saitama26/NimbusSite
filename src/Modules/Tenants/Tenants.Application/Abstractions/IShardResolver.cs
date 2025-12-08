namespace Tenants.Application.Abstractions;

/// <summary>
/// Абстракция для разрешения подключения (connection string) по TenantId.
/// Реализация будет в Infrastructure и может кешировать/использовать ShardMap.
/// </summary>
public interface IShardResolver
{
    /// <summary>
    /// Получить connection string по TenantId.
    /// Вернет null, если тенант не найден или не сконфигурирован.
    /// </summary>
    Task<string?> ResolveConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

