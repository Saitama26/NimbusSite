using Common.Infrastructure.Middleware;
using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;

namespace Projects.Infrastructure;

/// <summary>
/// Фабрика для создания ProjectsDbContext с динамическим connection string
/// Использует TenantConnectionCache для синхронного получения connection string
/// </summary>
public class TenantProjectsDbContextFactory : IDbContextFactory<ProjectsDbContext>
{
    private readonly TenantConnectionCache _connectionCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantProjectsDbContextFactory> _logger;

    public TenantProjectsDbContextFactory(
        TenantConnectionCache connectionCache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantProjectsDbContextFactory> logger)
    {
        _connectionCache = connectionCache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public ProjectsDbContext CreateDbContext()
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

        _logger.LogDebug("Creating ProjectsDbContext for TenantInt {TenantInt}", tenantInt.Value);

        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                opts => opts.SchemaBehavior(MySqlSchemaBehavior.Ignore))
            .Options;

        var dbContext = new ProjectsDbContext(options);
        
        // Применяем миграции при первом обращении к БД (lazy initialization)
        try
        {
            dbContext.Database.Migrate();
            _logger.LogDebug("Migrations applied successfully for ProjectsDbContext, TenantInt {TenantInt}", tenantInt.Value);
            
            // Создаем views после применения миграций (синхронный вызов асинхронного метода)
            var assembly = Assembly.GetExecutingAssembly();
            ViewInitializer.CreateViewsAsync(connectionString, assembly, _logger).GetAwaiter().GetResult();
            
            // Проверяем, что view доступна через DbContext
            try
            {
                // Пробуем выполнить простой запрос к view, чтобы убедиться, что она доступна
                var viewExists = dbContext.UserViews.Any();
                _logger.LogDebug("View vw_Users is accessible through DbContext (test query returned {Result})", viewExists);
            }
            catch (Exception viewCheckEx)
            {
                _logger.LogError(viewCheckEx, "View vw_Users is NOT accessible through DbContext after creation! " +
                                              "This may indicate a connection or naming issue.");
                // Не пробрасываем исключение, так как это может быть нормально для пустой view
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL: Failed to apply migrations or create views for ProjectsDbContext, TenantInt {TenantInt}. " +
                                 "This is a critical error that will prevent the application from working correctly.", tenantInt.Value);
            // Пробрасываем исключение, так как это критическая ошибка
            throw;
        }

        return dbContext;
    }
}
