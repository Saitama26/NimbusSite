using Common.Domain.Entities;

namespace Identity.Infrastructure.Persistence.Sharding;

/// <summary>
/// Запись в карте шардов для Identity
/// </summary>
public class ShardMapEntry : BaseEntity
{
    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Строка подключения к БД для этого тенанта
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    // EF Core требует конструктор без параметров
    public ShardMapEntry() : base() { }
}

