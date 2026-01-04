using Common.Infrastructure.Tenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации сервисов мультитенантности в DI
/// </summary>
public static class TenancyExtensions
{
    /// <summary>
    /// Добавить сервисы мультитенантности с кэшированием connection strings
    /// </summary>
    public static IServiceCollection AddTenancy(this IServiceCollection services, IConfiguration configuration)
    {
        // Регистрируем HttpContextAccessor
        services.AddHttpContextAccessor();

        // Регистрируем UserTenantService как singleton (без зависимостей от scoped сервисов)
        services.AddSingleton<UserTenantService>();

        // Регистрируем TenantConnectionCache как singleton
        services.AddSingleton(sp =>
        {
            var tenantsConnectionString = 
                Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
                ?? configuration.GetConnectionString("TenantsConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Tenants database connection string is not configured.");

            var logger = sp.GetRequiredService<ILogger<TenantConnectionCache>>();
            return new TenantConnectionCache(tenantsConnectionString, logger);
        });

        // Регистрируем EventHandlersTenantDbContextFactory для использования в event handlers
        // Использует TenantConnectionCache для получения connection string
        services.AddScoped<EventHandlersTenantDbContextFactory>();

        return services;
    }

    /// <summary>
    /// Инициализировать кэш connection strings (вызывать при старте приложения)
    /// </summary>
    public static async Task InitializeTenantCacheAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var cache = serviceProvider.GetRequiredService<TenantConnectionCache>();
        await cache.LoadAsync(cancellationToken);
    }
}
