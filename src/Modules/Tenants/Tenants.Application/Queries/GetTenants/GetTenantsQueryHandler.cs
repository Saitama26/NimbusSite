using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Application.Queries.GetTenants;
using Tenants.Domain.Enums;

namespace Tenants.Application.Queries.GetTenants;

/// <summary>
/// Обработчик запроса получения списка тенантов
/// </summary>
internal sealed class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, IEnumerable<TenantListItemDto>>
{
    private readonly ITenantsDbContext _dbContext;

    public GetTenantsQueryHandler(ITenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<TenantListItemDto>>> Handle(GetTenantsQuery query, CancellationToken cancellationToken)
    {
        var tenants = await _dbContext.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new TenantListItemDto(
                t.TenantInt,
                t.Name,
                t.Status,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<TenantListItemDto>>.Success(tenants);
    }
}

