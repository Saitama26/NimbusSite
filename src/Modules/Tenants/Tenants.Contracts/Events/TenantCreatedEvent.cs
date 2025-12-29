using Common.Domain.Events;
using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Events;

/// <summary>
/// Интеграционное событие создания нового тенанта
/// Тенант - это независимая сущность, связь пользователя с тенантом создается отдельно
/// </summary>
public sealed class TenantCreatedEvent : BaseIntegrationEvent
{
    public int TenantInt { get; }
    public string Name { get; }
    public TenantStatusContract Status { get; }
    public string? Description { get; }
    public string? ConnectionString { get; }

    public TenantCreatedEvent(
        int tenantInt,
        string name,
        TenantStatusContract status,
        string? description = null,
        string? connectionString = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantInt = tenantInt;
        Name = name;
        Status = status;
        Description = description;
        ConnectionString = connectionString;
    }
}

