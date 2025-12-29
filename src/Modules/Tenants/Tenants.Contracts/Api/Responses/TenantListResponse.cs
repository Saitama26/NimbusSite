using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Api.Responses;

/// <summary>
/// Ответ со списком тенантов (публичный API)
/// </summary>
public sealed record TenantListResponse(
    int TenantInt,
    string Name,
    TenantStatusContract Status,
    DateTime CreatedAt);

