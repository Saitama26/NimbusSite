namespace Tenants.Events;

/// <summary>
/// Событие: изменен статус тенанта.
/// </summary>
public sealed record TenantStatusChangedIntegrationEvent(
    Guid TenantId,
    string OldStatus,
    string NewStatus,
    DateTime OccurredAt);

