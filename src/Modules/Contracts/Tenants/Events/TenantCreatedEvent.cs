using Common.Domain.Events;

namespace Contracts.Tenants.Events;

/// <summary>
/// Интеграционное событие создания нового тенанта
/// </summary>
public sealed class TenantCreatedEvent : BaseDomainEvent
{
    public Guid TenantId { get; }
    public string Name { get; }
    public TenantStatusContract Status { get; }
    public string? Description { get; }
    public string? ConnectionString { get; }
    public Guid CreatedByUserId { get; }  // Кто создал тенант (пользователь, создавший первый проект)

    public TenantCreatedEvent(
        Guid tenantId,
        string name,
        TenantStatusContract status,
        Guid createdByUserId,
        string? description = null,
        string? connectionString = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantId = tenantId;
        Name = name;
        Status = status;
        CreatedByUserId = createdByUserId;
        Description = description;
        ConnectionString = connectionString;
    }
}

