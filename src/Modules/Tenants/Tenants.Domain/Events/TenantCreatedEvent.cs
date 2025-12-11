using Common.Domain.Events;
using Tenants.Domain.Enums;

namespace Tenants.Domain.Events;

/// <summary>
/// Интеграционное событие создания нового тенанта
/// </summary>
public sealed class TenantCreatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public string Subdomain { get; }
    public TenantStatus Status { get; }
    public string? Description { get; }
    public string? AdminEmail { get; }
    public string? ConnectionString { get; }

    public TenantCreatedEvent(
        Guid tenantId,
        string name,
        string subdomain,
        TenantStatus status,
        string? description = null,
        string? adminEmail = null,
        string? connectionString = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        Name = name;
        Subdomain = subdomain;
        Status = status;
        Description = description;
        AdminEmail = adminEmail;
        ConnectionString = connectionString;
    }
}

