using Common.Application.Abstractions.Messaging;
using Tenants.Application.DTOs;

namespace Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Запрос получения тенанта по ID
/// </summary>
public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantDto>;

