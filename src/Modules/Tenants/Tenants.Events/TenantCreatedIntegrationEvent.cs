namespace Tenants.Events;

/// <summary>
/// Событие: создан тенант.
/// </summary>
public sealed record TenantCreatedIntegrationEvent(
    Guid TenantId,
    string Name,
    string Subdomain,
    string Status,
    string? ShardKey,
    DateTime OccurredAt);

