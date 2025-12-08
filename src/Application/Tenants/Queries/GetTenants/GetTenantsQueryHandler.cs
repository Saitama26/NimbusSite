using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Tenants.Queries.GetTenants;

internal sealed class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, IReadOnlyList<TenantResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<TenantResponse>>> Handle(GetTenantsQuery query, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .ToListAsync(cancellationToken);

        var response = tenants.Select(tenant => new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.ConnectionString,
            tenant.CreatedAt
        )).ToList();

        return response;
    }
}

