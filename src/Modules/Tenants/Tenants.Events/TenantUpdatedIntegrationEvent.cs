namespace Tenants.Events;

/// <summary>
/// Событие: обновлены данные тенанта.
/// </summary>
public sealed record TenantUpdatedIntegrationEvent(
    Guid TenantId,
    string? Name,
    string? Description,
    string? AdminEmail,
    string? Status,
    DateTime OccurredAt);

