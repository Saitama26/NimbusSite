using Common.Domain.Events;

namespace Tenants.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления тенанта (soft delete)
/// </summary>
public sealed class TenantDeletedEvent : BaseIntegrationEvent
{
    public int TenantInt { get; }

    public TenantDeletedEvent(int tenantInt, DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantInt = tenantInt;
    }
}

