namespace Application.Tenants.Queries.GetTenantById;

public sealed record TenantResponse(
    Guid TenantId,
    string Name,
    string ConnectionString,
    DateTime CreatedAt) { }

