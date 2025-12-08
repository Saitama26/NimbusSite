using Tenants.Domain.Enums;

namespace Tenants.Application.DTOs;

/// <summary>
/// DTO для полной информации о тенанте
/// </summary>
public sealed record TenantDto(
    Guid Id,
    string Name,
    string Subdomain,
    string? ConnectionString,
    TenantStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Description,
    string? AdminEmail);

