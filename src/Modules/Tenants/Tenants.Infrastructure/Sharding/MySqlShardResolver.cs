using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;
using Tenants.Infrastructure.Persistence;
using Tenants.Infrastructure.Persistence.Sharding;

namespace Tenants.Infrastructure.Sharding;

/// <summary>
/// Простая реализация резолвера подключения: ищет в ShardMap, затем в Tenant.ConnectionString.
/// </summary>
internal sealed class MySqlShardResolver : IShardResolver
{
    private readonly TenantsDbContext _dbContext;

    public MySqlShardResolver(TenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> ResolveConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        // 1. Пробуем карту шардов
        var shardEntry = await _dbContext.ShardMapEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        if (shardEntry != null && !string.IsNullOrWhiteSpace(shardEntry.ConnectionString))
        {
            return shardEntry.ConnectionString;
        }

        // 2. Фоллбек: берем connection string из самого тенанта
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);

        return tenant?.ConnectionString;
    }
}

