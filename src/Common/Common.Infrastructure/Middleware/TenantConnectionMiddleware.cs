using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для получения connection string тенанта
/// Должен быть зарегистрирован ПОСЛЕ TenantContextMiddleware
/// Получает connection string из TenantConnectionCache и кладет в HttpContext.Items
/// </summary>
public sealed class TenantConnectionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantConnectionMiddleware> _logger;
    private const string ConnectionStringItemKey = "TenantConnectionString";

    public TenantConnectionMiddleware(
        RequestDelegate next,
        ILogger<TenantConnectionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantInt = TenantContextMiddleware.GetTenantInt(context);
        
        if (tenantInt.HasValue)
        {
            try
            {
                // Получаем TenantConnectionCache из services (singleton)
                var connectionCache = context.RequestServices.GetRequiredService<TenantConnectionCache>();
                
                // Синхронно получаем connection string из кэша
                var connectionString = connectionCache.GetConnectionString(tenantInt.Value);
                
                // Сохраняем в HttpContext.Items
                context.Items[ConnectionStringItemKey] = connectionString;
                
                _logger.LogDebug("Tenant connection string resolved for TenantInt {TenantInt}", tenantInt.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve connection string for TenantInt {TenantInt}", tenantInt.Value);
                throw;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Получить connection string тенанта из HttpContext
    /// </summary>
    public static string? GetConnectionString(HttpContext context)
    {
        return context.Items.TryGetValue(ConnectionStringItemKey, out var value) && value is string connectionString
            ? connectionString
            : null;
    }
}


