using SharedKernel;

namespace Domain.Tenants.Events;

public sealed record TenantCreatedEvent(Guid tenantId, string name) : IDomainEvent { }