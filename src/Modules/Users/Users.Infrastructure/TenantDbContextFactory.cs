using Common.Infrastructure.Middleware;
using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Users.Infrastructure;

/// <summary>
/// Фабрика для создания UsersDbContext с динамическим connection string
/// Использует TenantConnectionCache для синхронного получения connection string
/// </summary>
public class TenantUsersDbContextFactory : IDbContextFactory<UsersDbContext>
{
    private readonly TenantConnectionCache _connectionCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantUsersDbContextFactory> _logger;

    public TenantUsersDbContextFactory(
        TenantConnectionCache connectionCache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantUsersDbContextFactory> logger)
    {
        _connectionCache = connectionCache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public UsersDbContext CreateDbContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        // Получаем TenantInt из HttpContext (установлен TenantContextMiddleware)
        var tenantInt = TenantContextMiddleware.GetTenantInt(httpContext);
        if (!tenantInt.HasValue)
        {
            throw new InvalidOperationException("TenantId is not set. Provide tenantId query parameter or X-Tenant-Id header.");
        }

        // Получаем connection string из кэша (синхронно!)
        var connectionString = _connectionCache.GetConnectionString(tenantInt.Value);

        _logger.LogDebug("Creating UsersDbContext for TenantInt {TenantInt}", tenantInt.Value);

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                opts => opts.SchemaBehavior(MySqlSchemaBehavior.Ignore))
            .Options;

        var dbContext = new UsersDbContext(options);
        
        // Применяем миграции при первом обращении к БД (lazy initialization)
        try
        {
            dbContext.Database.Migrate();
            _logger.LogDebug("Migrations applied successfully for UsersDbContext, TenantInt {TenantInt}", tenantInt.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply migrations for UsersDbContext, TenantInt {TenantInt}. " +
                                  "This may be normal if migrations are already applied.", tenantInt.Value);
        }

        return dbContext;
    }
}
