namespace Tenants.Events;

/// <summary>
/// Событие: обновлена информация о подключении (без явного connection string).
/// </summary>
public sealed record TenantConnectionInfoChangedIntegrationEvent(
    Guid TenantId,
    string? ShardKey,
    DateTime OccurredAt);

