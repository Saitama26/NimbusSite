namespace Users.Infrastructure.Persistence.Sharding;

/// <summary>
/// Запись карты шардов для Users.
/// </summary>
public class ShardMapEntry
{
    public Guid TenantId { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string? ShardKey { get; set; }
}

