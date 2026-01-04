using Common.Application.Abstractions.Messaging;
using Tenants.Domain.Enums;

namespace Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Запрос получения тенанта по числовому идентификатору
/// </summary>
public sealed record GetTenantByIdQuery(int TenantInt) : IQuery<TenantDto>;

/// <summary>
/// DTO для тенанта (внутренний)
/// </summary>
public sealed record TenantDto(
    int TenantInt,
    string Name,
    TenantStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Description = null,
    string? ConnectionString = null);

