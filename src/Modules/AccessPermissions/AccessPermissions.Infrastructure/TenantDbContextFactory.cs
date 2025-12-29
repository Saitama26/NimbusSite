using Common.Infrastructure.Middleware;
using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;

namespace AccessPermissions.Infrastructure;

/// <summary>
/// Фабрика для создания AccessPermissionsDbContext с динамическим connection string
/// Использует TenantConnectionCache для синхронного получения connection string
/// </summary>
public class TenantAccessPermissionsDbContextFactory : IDbContextFactory<AccessPermissionsDbContext>
{
    private readonly TenantConnectionCache _connectionCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantAccessPermissionsDbContextFactory> _logger;

    public TenantAccessPermissionsDbContextFactory(
        TenantConnectionCache connectionCache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantAccessPermissionsDbContextFactory> logger)
    {
        _connectionCache = connectionCache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public AccessPermissionsDbContext CreateDbContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        var tenantInt = TenantContextMiddleware.GetTenantInt(httpContext);
        if (!tenantInt.HasValue)
        {
            throw new InvalidOperationException("TenantId is not set. Provide tenantId query parameter or X-Tenant-Id header.");
        }

        var connectionString = _connectionCache.GetConnectionString(tenantInt.Value);

        _logger.LogDebug("Creating AccessPermissionsDbContext for TenantInt {TenantInt}", tenantInt.Value);

        var options = new DbContextOptionsBuilder<AccessPermissionsDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                opts => opts.SchemaBehavior(MySqlSchemaBehavior.Ignore))
            .Options;

        var dbContext = new AccessPermissionsDbContext(options);
        
        // Применяем миграции при первом обращении к БД (lazy initialization)
        try
        {
            dbContext.Database.Migrate();
            _logger.LogDebug("Migrations applied successfully for AccessPermissionsDbContext, TenantInt {TenantInt}", tenantInt.Value);
            
            // Создаем views после применения миграций (синхронный вызов асинхронного метода)
            var assembly = Assembly.GetExecutingAssembly();
            ViewInitializer.CreateViewsAsync(connectionString, assembly, _logger).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply migrations or create views for AccessPermissionsDbContext, TenantInt {TenantInt}. " +
                                  "This may be normal if migrations/views are already applied.", tenantInt.Value);
        }

        return dbContext;
    }
}
