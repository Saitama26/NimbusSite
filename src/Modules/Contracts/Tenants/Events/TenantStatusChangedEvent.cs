using Common.Domain.Events;

namespace Contracts.Tenants.Events;

/// <summary>
/// Интеграционное событие изменения статуса тенанта
/// </summary>
public sealed class TenantStatusChangedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public TenantStatusContract OldStatus { get; }
    public TenantStatusContract NewStatus { get; }

    public TenantStatusChangedEvent(
        Guid tenantId,
        TenantStatusContract oldStatus,
        TenantStatusContract newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

