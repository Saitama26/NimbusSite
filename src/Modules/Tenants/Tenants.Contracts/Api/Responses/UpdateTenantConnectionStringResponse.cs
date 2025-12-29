namespace Tenants.Contracts.Api.Responses;

/// <summary>
/// Ответ при обновлении connection string тенанта
/// </summary>
public sealed record UpdateTenantConnectionStringResponse(
    int TenantInt,
    string ConnectionString);

