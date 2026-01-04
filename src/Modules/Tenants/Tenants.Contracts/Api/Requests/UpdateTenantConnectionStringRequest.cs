namespace Tenants.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление connection string тенанта (публичный API)
/// </summary>
public sealed record UpdateTenantConnectionStringRequest(
    string ConnectionString);

