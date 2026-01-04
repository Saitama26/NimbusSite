using Common.Application.Abstractions.Messaging;
using Tenants.Domain.Enums;

namespace Tenants.Application.Queries.GetTenants;

/// <summary>
/// Запрос получения списка тенантов
/// </summary>
public sealed record GetTenantsQuery() : IQuery<IEnumerable<TenantListItemDto>>;

/// <summary>
/// DTO для списка тенантов (внутренний)
/// </summary>
public sealed record TenantListItemDto(
    int TenantInt,
    string Name,
    TenantStatus Status,
    DateTime CreatedAt);

