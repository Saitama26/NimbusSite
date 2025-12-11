using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions;
using Projects.Infrastructure.Persistence.Sharding;

namespace Projects.Infrastructure.Sharding;

/// <summary>
/// Резолвер connection string по TenantId для Projects.
/// 1) Пытается найти запись в ShardMap.
/// 2) Fallback: возвращает дефолтное подключение (можно расширить на чтение из кэша/конфигурации).
/// </summary>
internal sealed class MySqlShardResolver : IShardResolver
{
    private readonly ProjectsDbContext _dbContext;
    private readonly string _defaultConnectionString;

    public MySqlShardResolver(ProjectsDbContext dbContext, string defaultConnectionString)
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

        // fallback: использовать дефолтное подключение (общая БД проектов)
        return _defaultConnectionString;
    }
}

