using Application.Abstractions.Messaging;

namespace Application.Tenants.Queries.GetTenantById;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantResponse> { }

