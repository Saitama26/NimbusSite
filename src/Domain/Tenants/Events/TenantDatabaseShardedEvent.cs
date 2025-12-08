using SharedKernel;

namespace Domain.Tenants.Events;

public sealed record TenantDatabaseShardedEvent(Guid tenantId, string connectionString) : IDomainEvent { }