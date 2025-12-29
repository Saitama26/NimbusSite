using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Application.Queries.GetTenantById;
using Tenants.Domain.Errors;

namespace Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Обработчик запроса получения тенанта по числовому идентификатору
/// </summary>
internal sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly ITenantsDbContext _dbContext;

    public GetTenantByIdQueryHandler(ITenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery query, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantInt == query.TenantInt, cancellationToken);

        if (tenant == null)
        {
            return Result<TenantDto>.Failure(TenantErrors.NotFound(query.TenantInt));
        }

        var dto = new TenantDto(
            tenant.TenantInt,
            tenant.Name,
            tenant.Status,
            tenant.CreatedAt,
            tenant.UpdatedAt,
            tenant.Description,
            tenant.ConnectionString);

        return Result<TenantDto>.Success(dto);
    }
}

