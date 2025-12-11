using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Интеграционное событие обновления строки подключения тенанта
/// </summary>
public sealed class TenantConnectionStringUpdatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string? ConnectionString { get; }

    public TenantConnectionStringUpdatedEvent(
        Guid tenantId,
        string? connectionString = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        ConnectionString = connectionString;
    }
}

