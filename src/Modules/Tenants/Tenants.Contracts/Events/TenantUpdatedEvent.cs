using Common.Domain.Events;

namespace Tenants.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления тенанта
/// </summary>
public sealed class TenantUpdatedEvent : BaseIntegrationEvent
{
    public int TenantInt { get; }
    public string? Name { get; }
    public string? Description { get; }

    public TenantUpdatedEvent(
        int tenantInt,
        string? name = null,
        string? description = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantInt = tenantInt;
        Name = name;
        Description = description;
    }
}

