using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление тенанта (публичный API)
/// </summary>
public sealed record UpdateTenantRequest(
    string Name,
    string? Description = null,
    TenantStatusContract? Status = null);

