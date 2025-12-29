using Common.Domain.Events;
using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения статуса тенанта
/// </summary>
public sealed class TenantStatusChangedEvent : BaseIntegrationEvent
{
    public int TenantInt { get; }
    public TenantStatusContract OldStatus { get; }
    public TenantStatusContract NewStatus { get; }

    public TenantStatusChangedEvent(
        int tenantInt,
        TenantStatusContract oldStatus,
        TenantStatusContract newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantInt = tenantInt;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

