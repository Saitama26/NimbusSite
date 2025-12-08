using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Событие обновления строки подключения тенанта
/// </summary>
public sealed class TenantConnectionStringUpdatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }

    public TenantConnectionStringUpdatedEvent(Guid tenantId, DateTime updatedAt)
        : base(Guid.NewGuid(), updatedAt)
    {
        TenantId = tenantId;
    }
}

