using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Событие удаления тенанта (soft delete)
/// </summary>
public sealed class TenantDeletedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }

    public TenantDeletedEvent(Guid tenantId, DateTime deletedAt)
        : base(Guid.NewGuid(), deletedAt)
    {
        TenantId = tenantId;
    }
}

