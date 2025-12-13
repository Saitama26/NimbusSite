using Microsoft.EntityFrameworkCore;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence.Sharding;

namespace Identity.Infrastructure.Sharding;

/// <summary>
/// Резолвер connection string по TenantId для Identity
/// </summary>
internal sealed class MySqlShardResolver : IShardResolver
{
    private readonly IdentityDbContext _dbContext;
    private readonly string _defaultConnectionString;

    public MySqlShardResolver(IdentityDbContext dbContext, string defaultConnectionString)
    {
        _dbContext = dbContext;
        _defaultConnectionString = defaultConnectionString;
    }

    public async Task<string?> ResolveConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var shardEntry = await _dbContext.ShardMapEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (shardEntry != null && !string.IsNullOrWhiteSpace(shardEntry.ConnectionString))
        {
            return shardEntry.ConnectionString;
        }

        // fallback: использовать дефолтное подключение
        return _defaultConnectionString;
    }
}

