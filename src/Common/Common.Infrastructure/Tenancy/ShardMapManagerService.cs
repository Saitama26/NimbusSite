using Common.Application.Abstractions.Tenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Менеджер для получения connection string тенанта по его числовому идентификатору
/// Использует Shard Map для маппинга tenantInt → connection string
/// Использует кэширование (TTL 5 минут) для оптимизации производительности
/// </summary>
internal sealed class ShardMapManagerService : IShardMapManager
{
    private readonly ITenancyDomain _tenancyDomain;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ShardMapManagerService> _logger;
    private const int CacheTtlMinutes = 5;
    private const string CacheKeyPrefix = "tenant_connection_";

    public ShardMapManagerService(
        ITenancyDomain tenancyDomain,
        IMemoryCache cache,
        ILogger<ShardMapManagerService> logger)
    {
        _tenancyDomain = tenancyDomain;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetConnectionStringAsync(int tenantInt, CancellationToken cancellationToken = default)
    {
        if (tenantInt <= 0)
        {
            throw new ArgumentException("TenantInt must be greater than zero", nameof(tenantInt));
        }

        var cacheKey = $"{CacheKeyPrefix}{tenantInt}";

        // Проверяем кэш
        if (_cache.TryGetValue(cacheKey, out string? cachedConnectionString) && !string.IsNullOrEmpty(cachedConnectionString))
        {
            _logger.LogDebug("Connection string for TenantInt {TenantInt} found in cache", tenantInt);
            return cachedConnectionString;
        }

        // Если нет в кэше, получаем из TenancyDomain
        _logger.LogDebug("Connection string for TenantInt {TenantInt} not found in cache, fetching from TenancyDomain", tenantInt);
        var tenantInfo = await _tenancyDomain.GetTenantInfoAsync(tenantInt, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrEmpty(tenantInfo.ConnectionString))
        {
            throw new InvalidOperationException($"Connection string not found for TenantInt {tenantInt}");
        }

        // Кэшируем результат на 5 минут
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTtlMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(CacheTtlMinutes)
        };
        _cache.Set(cacheKey, tenantInfo.ConnectionString, cacheOptions);

        _logger.LogInformation("Connection string for TenantInt {TenantInt} cached", tenantInt);
        return tenantInfo.ConnectionString;
    }
}

