using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Tenancy;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;

namespace Tenants.Application.Queries.GetTenantInfo;

/// <summary>
/// Запрос для получения информации о тенанте по числовому идентификатору
/// Используется как API Function для ITenancyDomain
/// </summary>
internal sealed record GetTenantInfoQuery(int TenantInt) : IQuery<TenantInfo>;

/// <summary>
/// Обработчик запроса GetTenantInfo
/// </summary>
internal sealed class GetTenantInfoQueryHandler : IQueryHandler<GetTenantInfoQuery, TenantInfo>
{
    private readonly ITenantsDbContext _dbContext;

    public GetTenantInfoQueryHandler(ITenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<TenantInfo>> Handle(GetTenantInfoQuery query, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantInt == query.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result<TenantInfo>.Failure(Error.NotFound("Tenant.NotFound", $"Tenant with TenantInt '{query.TenantInt}' not found"));
        }

        var tenantInfo = new TenantInfo
        {
            TenantInt = tenant.TenantInt,
            Name = tenant.Name,
            ConnectionString = tenant.ConnectionString,
            CreatedAt = tenant.CreatedAt
        };

        return Result<TenantInfo>.Success(tenantInfo);
    }
}

