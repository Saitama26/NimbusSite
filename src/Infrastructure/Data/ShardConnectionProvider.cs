using Application.Abstractions.Data;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class ShardConnectionProvider : IShardConnectionProvider
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ShardConnectionProvider> _logger;

    public ShardConnectionProvider(
        IApplicationDbContext context,
        ITenantContext tenantContext,
        ILogger<ShardConnectionProvider> logger)
    {
        _context = context;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            _logger.LogWarning("Tenant {TenantId} not found", tenantId);
            throw new InvalidOperationException($"Tenant {tenantId} not found");
        }

        return tenant.ConnectionString;
    }

    public async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            throw new InvalidOperationException("TenantId is not set in context");
        }

        return await GetConnectionStringAsync(_tenantContext.TenantId.Value, cancellationToken);
    }
}

