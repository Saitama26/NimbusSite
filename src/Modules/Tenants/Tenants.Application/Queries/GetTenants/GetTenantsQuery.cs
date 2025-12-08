using Common.Application.Abstractions.Messaging;
using Tenants.Application.DTOs;

namespace Tenants.Application.Queries.GetTenants;

/// <summary>
/// Запрос получения списка тенантов
/// OData обрабатывает пагинацию, фильтрацию и сортировку через запросы ($skip, $top, $filter, $orderby)
/// </summary>
public sealed record GetTenantsQuery() : IQuery<IQueryable<TenantListItemDto>>;

