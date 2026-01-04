using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Универсальная фабрика для создания DbContext с TenantId для event handlers
/// Используется в event handlers, где нет HttpContext, но есть TenantId из события
/// Использует TenantConnectionCache для получения connection string
/// </summary>
public class EventHandlersTenantDbContextFactory
{
    private readonly TenantConnectionCache _connectionCache;
    private readonly ILogger<EventHandlersTenantDbContextFactory> _logger;

    public EventHandlersTenantDbContextFactory(
        TenantConnectionCache connectionCache,
        ILogger<EventHandlersTenantDbContextFactory> logger)
    {
        _connectionCache = connectionCache;
        _logger = logger;
    }

    /// <summary>
    /// Создать DbContext для конкретного тенанта (используется в event handlers)
    /// Применяет миграции автоматически при первом обращении к БД
    /// </summary>
    public async Task<TContext> CreateDbContextForTenantAsync<TContext>(
        int tenantInt,
        Func<DbContextOptions<TContext>, TContext> contextFactory,
        CancellationToken cancellationToken = default) where TContext : DbContext
    {
        if (tenantInt <= 0)
        {
            throw new ArgumentException("TenantInt must be greater than zero", nameof(tenantInt));
        }

        // Получаем connection string для тенанта из кэша (синхронно)
        var connectionString = _connectionCache.GetConnectionString(tenantInt);
        
        _logger.LogDebug("Creating {ContextType} for TenantInt {TenantInt}", typeof(TContext).Name, tenantInt);

        // Создаем опции с connection string тенанта
        var options = new DbContextOptionsBuilder<TContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                opts => opts.SchemaBehavior(MySqlSchemaBehavior.Ignore))
            .Options;

        // Создаем экземпляр DbContext через фабрику
        var dbContext = contextFactory(options);
        
        // Применяем миграции при первом обращении к БД (lazy initialization)
        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            _logger.LogDebug("Migrations applied successfully for {ContextType}, TenantInt {TenantInt}", typeof(TContext).Name, tenantInt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply migrations for {ContextType}, TenantInt {TenantInt}. " +
                                  "This may be normal if migrations are already applied.", typeof(TContext).Name, tenantInt);
        }

        return dbContext;
    }
}

