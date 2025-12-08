namespace Application.Tenants.Queries.GetTenants;

public sealed record TenantResponse(
    Guid TenantId,
    string Name,
    string ConnectionString,
    DateTime CreatedAt) { }

