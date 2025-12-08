namespace Tenants.Events;

/// <summary>
/// Событие: тенант удалён (soft delete).
/// </summary>
public sealed record TenantDeletedIntegrationEvent(
    Guid TenantId,
    DateTime OccurredAt);

