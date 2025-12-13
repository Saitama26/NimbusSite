using Common.Domain.Events;

namespace Contracts.Tenants.Events;

/// <summary>
/// Интеграционное событие удаления тенанта (soft delete)
/// </summary>
public sealed class TenantDeletedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }

    public TenantDeletedEvent(Guid tenantId, DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
    }
}

