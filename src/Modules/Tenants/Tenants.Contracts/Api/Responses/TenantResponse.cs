using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Api.Responses;

/// <summary>
/// Ответ с информацией о тенанте (публичный API)
/// </summary>
public sealed record TenantResponse(
    int TenantInt,
    string Name,
    TenantStatusContract Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Description = null,
    string? ConnectionString = null);

