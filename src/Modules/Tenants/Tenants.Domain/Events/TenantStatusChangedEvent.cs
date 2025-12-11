using Common.Domain.Events;
using Tenants.Domain.Enums;

namespace Tenants.Domain.Events;

/// <summary>
/// Интеграционное событие изменения статуса тенанта
/// </summary>
public sealed class TenantStatusChangedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public TenantStatus OldStatus { get; }
    public TenantStatus NewStatus { get; }

    public TenantStatusChangedEvent(
        Guid tenantId,
        TenantStatus oldStatus,
        TenantStatus newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

