using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Tenancy;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tenants.Application.Abstractions;
using Tenants.Contracts.Events;
using Tenants.Infrastructure.EventHandlers;
using Tenants.Infrastructure.Persistence;
using Tenants.Infrastructure.Tenancy;
using Tenants.Infrastructure;

namespace Tenants.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Tenants: DbContext, UoW, TenancyDomain.
    /// Работает с центральной БД NimbusSite_Tenants (таблицы в корне базы данных без схем)
    /// </summary>
    public static IServiceCollection AddTenantsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (включая TenantConnectionCache)
        services.AddTenancy(configuration);

        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'TENANTS_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<TenantsDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.Parse("8.0.0-mysql")));

        // Регистрируем ITenantsDbContext для использования в Application слое
        services.AddScoped<ITenantsDbContext>(sp => sp.GetRequiredService<TenantsDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Регистрируем ITenancyDomain для мультитенантности
        services.AddScoped<ITenancyDomain, TenancyDomain>();

        // Регистрируем TenantDatabaseInitializer для инициализации БД тенантов
        services.AddSingleton<Common.Infrastructure.Tenancy.TenantDatabaseInitializer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Common.Infrastructure.Tenancy.TenantDatabaseInitializer>>();
            return new Common.Infrastructure.Tenancy.TenantDatabaseInitializer(connectionString, logger);
        });

        // Регистрируем event handlers
        services.AddScoped<IEventHandler<TenantCreatedEvent>, TenantCreatedEventHandler>();

        return services;
    }
}

