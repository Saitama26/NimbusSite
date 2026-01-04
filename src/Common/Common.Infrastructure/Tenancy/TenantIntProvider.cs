using Common.Application.Abstractions.Tenancy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Провайдер для получения числового идентификатора тенанта по его имени
/// Использует кэширование (TTL 5 минут) для оптимизации производительности
/// </summary>
internal sealed class TenantIntProvider : ITenantIntProvider
{
    private readonly ITenancyDomain _tenancyDomain;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantIntProvider> _logger;
    private const int CacheTtlMinutes = 5;
    private const string CacheKeyPrefix = "tenant_int_";

    public TenantIntProvider(
        ITenancyDomain tenancyDomain,
        IMemoryCache cache,
        ILogger<TenantIntProvider> logger)
    {
        _tenancyDomain = tenancyDomain;
        _cache = cache;
        _logger = logger;
    }

    public async Task<int> GetTenantIntAsync(string tenantName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantName))
        {
            throw new ArgumentException("Tenant name cannot be null or empty", nameof(tenantName));
        }

        var cacheKey = $"{CacheKeyPrefix}{tenantName.ToLowerInvariant()}";

        // Проверяем кэш
        if (_cache.TryGetValue(cacheKey, out int cachedTenantInt))
        {
            _logger.LogDebug("TenantInt for {TenantName} found in cache: {TenantInt}", tenantName, cachedTenantInt);
            return cachedTenantInt;
        }

        // Если нет в кэше, получаем из TenancyDomain
        _logger.LogDebug("TenantInt for {TenantName} not found in cache, fetching from TenancyDomain", tenantName);
        var tenantInt = await _tenancyDomain.FindTenantIntAsync(tenantName, cancellationToken).ConfigureAwait(false);

        // Кэшируем результат на 5 минут
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTtlMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(CacheTtlMinutes)
        };
        _cache.Set(cacheKey, tenantInt, cacheOptions);

        _logger.LogInformation("TenantInt for {TenantName} cached: {TenantInt}", tenantName, tenantInt);
        return tenantInt;
    }
}

