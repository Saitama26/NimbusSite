namespace Projects.Infrastructure.Persistence.Sharding;

/// <summary>
/// Запись карты шардов для проектов: TenantId -> ConnectionString (опционально ShardKey).
/// </summary>
public class ShardMapEntry
{
    public Guid TenantId { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string? ShardKey { get; set; }
}

