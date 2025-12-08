using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Tenants.Errors;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.Queries.GetTenantById;

internal sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    private readonly IApplicationDbContext _context;

    public GetTenantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TenantResponse>> Handle(GetTenantByIdQuery query, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == query.TenantId, cancellationToken);

        if (tenant is null)
        {
            return TenantErrors.NotFound(query.TenantId);
        }

        return new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.ConnectionString,
            tenant.CreatedAt);
    }
}

