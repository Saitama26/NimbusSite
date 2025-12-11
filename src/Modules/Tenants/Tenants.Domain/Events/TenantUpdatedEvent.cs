using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Интеграционное событие обновления тенанта
/// </summary>
public sealed class TenantUpdatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string? Name { get; }
    public string? Description { get; }
    public string? AdminEmail { get; }

    public TenantUpdatedEvent(
        Guid tenantId,
        string? name = null,
        string? description = null,
        string? adminEmail = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        AdminEmail = adminEmail;
    }
}

