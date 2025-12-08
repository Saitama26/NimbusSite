using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Событие обновления тенанта
/// </summary>
public sealed class TenantUpdatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }

    public TenantUpdatedEvent(Guid tenantId, string name, DateTime updatedAt)
        : base(Guid.NewGuid(), updatedAt)
    {
        TenantId = tenantId;
        Name = name;
    }
}

