using Common.Infrastructure.Middleware;
using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;

namespace Tasks.Infrastructure;

/// <summary>
/// Фабрика для создания TasksDbContext с динамическим connection string
/// Использует TenantConnectionCache для синхронного получения connection string
/// </summary>
public class TenantTasksDbContextFactory : IDbContextFactory<TasksDbContext>
{
    private readonly TenantConnectionCache _connectionCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantTasksDbContextFactory> _logger;

    public TenantTasksDbContextFactory(
        TenantConnectionCache connectionCache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantTasksDbContextFactory> logger)
    {
        _connectionCache = connectionCache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public TasksDbContext CreateDbContext()
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

        _logger.LogDebug("Creating TasksDbContext for TenantInt {TenantInt}", tenantInt.Value);

        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                opts => opts.SchemaBehavior(MySqlSchemaBehavior.Ignore))
            .Options;

        var dbContext = new TasksDbContext(options);
        
        // Применяем миграции при первом обращении к БД (lazy initialization)
        try
        {
            dbContext.Database.Migrate();
            _logger.LogDebug("Migrations applied successfully for TasksDbContext, TenantInt {TenantInt}", tenantInt.Value);
            
            // Создаем views после применения миграций (синхронный вызов асинхронного метода)
            var assembly = Assembly.GetExecutingAssembly();
            ViewInitializer.CreateViewsAsync(connectionString, assembly, _logger).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply migrations or create views for TasksDbContext, TenantInt {TenantInt}. " +
                                  "This may be normal if migrations/views are already applied.", tenantInt.Value);
        }

        return dbContext;
    }
}
