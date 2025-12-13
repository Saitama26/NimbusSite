using Microsoft.EntityFrameworkCore;
using Tasks.Application.Abstractions;
using Tasks.Infrastructure.Persistence.Sharding;

namespace Tasks.Infrastructure.Sharding;

/// <summary>
/// Резолвер connection string по TenantId для Tasks
/// 1) Пытается найти запись в ShardMap
/// 2) Fallback: возвращает дефолтное подключение
/// </summary>
internal sealed class MySqlShardResolver : IShardResolver
{
    private readonly TasksDbContext _dbContext;
    private readonly string _defaultConnectionString;

    public MySqlShardResolver(TasksDbContext dbContext, string defaultConnectionString)
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

        // fallback: использовать дефолтное подключение (общая БД задач)
        return _defaultConnectionString;
    }
}

