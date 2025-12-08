using Tenants.Domain.Enums;

namespace Tenants.Application.DTOs;

/// <summary>
/// DTO для краткой информации о тенанте в списке
/// </summary>
public sealed record TenantListItemDto(
    Guid Id,
    string Name,
    string Subdomain,
    TenantStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

