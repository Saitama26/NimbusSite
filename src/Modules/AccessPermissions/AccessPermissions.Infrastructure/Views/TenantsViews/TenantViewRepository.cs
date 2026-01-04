using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AccessPermissions.Application.Abstractions.Views;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Infrastructure.Views.TenantsViews;

internal sealed class TenantViewRepository : ITenantViewRepository
{
    private readonly AccessPermissionsDbContext _dbContext;

    public TenantViewRepository(AccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantViewDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantViews
            .AsNoTracking()
            .Where(x => x.Id == tenantId)
            .Select(x => new TenantViewDto
            {
                Id = x.Id,
                Name = x.Name,
                Status = x.Status,
                Subdomain = x.Subdomain,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TenantViews
            .AsNoTracking()
            .AnyAsync(x => x.Id == tenantId, cancellationToken);
    }
}

