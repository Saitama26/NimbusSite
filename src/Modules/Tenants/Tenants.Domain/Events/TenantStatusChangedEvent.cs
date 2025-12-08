using Common.Domain.Events;
using Tenants.Domain.Enums;

namespace Tenants.Domain.Events;

/// <summary>
/// Событие изменения статуса тенанта
/// </summary>
public sealed class TenantStatusChangedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public TenantStatus OldStatus { get; }
    public TenantStatus NewStatus { get; }

    public TenantStatusChangedEvent(Guid tenantId, TenantStatus oldStatus, TenantStatus newStatus, DateTime changedAt)
        : base(Guid.NewGuid(), changedAt)
    {
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

