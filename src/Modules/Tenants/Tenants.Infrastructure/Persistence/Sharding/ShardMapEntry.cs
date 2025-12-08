namespace Tenants.Infrastructure.Persistence.Sharding;

/// <summary>
/// Запись карты шардов: сопоставление TenantId -> ConnectionString (или ShardKey).
/// Хранится в каталожной БД (TenantsDbContext).
/// </summary>
public class ShardMapEntry
{
    public Guid TenantId { get; set; }

    /// <summary>
    /// Connection string к БД конкретного тенанта/шарда.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Дополнительный ключ шарда (опционально, если нужно).
    /// </summary>
    public string? ShardKey { get; set; }
}

