using Common.Domain.Events;

namespace Tenants.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления connection string тенанта
/// </summary>
public sealed class TenantConnectionStringUpdatedEvent : BaseIntegrationEvent
{
    public int TenantInt { get; }
    public string ConnectionString { get; }

    public TenantConnectionStringUpdatedEvent(
        int tenantInt,
        string connectionString,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TenantInt = tenantInt;
        ConnectionString = connectionString;
    }
}

