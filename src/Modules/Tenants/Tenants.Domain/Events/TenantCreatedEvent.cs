using Common.Domain.Events;

namespace Tenants.Domain.Events;

/// <summary>
/// Событие создания нового тенанта
/// </summary>
public sealed class TenantCreatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public string Subdomain { get; }

    public TenantCreatedEvent(Guid tenantId, string name, string subdomain, DateTime createdAt)
        : base(Guid.NewGuid(), createdAt)
    {
        TenantId = tenantId;
        Name = name;
        Subdomain = subdomain;
    }
}

