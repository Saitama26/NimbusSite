using Application.Abstractions.Messaging;

namespace Application.Tenants.Queries.GetTenants;

public sealed record GetTenantsQuery() : IQuery<IReadOnlyList<TenantResponse>> { }

