using SharedKernel;

namespace Domain.Tenants.Events;

public sealed record TenantUpdatedEvent(Guid tenantId, string name) : IDomainEvent { }

