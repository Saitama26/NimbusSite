using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Infrastructure.Persistence.Sharding;

namespace Users.Infrastructure.Sharding;

/// <summary>
/// Резолвер connection string по TenantId для Users.
/// 1) Пытается найти запись в ShardMap.
/// 2) Fallback: возвращает дефолтное подключение.
/// </summary>
internal sealed class MySqlShardResolver : IShardResolver
{
    private readonly UsersDbContext _dbContext;
    private readonly string _defaultConnectionString;

    public MySqlShardResolver(UsersDbContext dbContext, string defaultConnectionString)
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

        // fallback: использовать дефолтное подключение (общая БД пользователей)
        return _defaultConnectionString;
    }
}

