using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenants.Application.Abstractions;
using Tenants.Infrastructure.Persistence;
using Tenants.Infrastructure.Repositories;
using Tenants.Infrastructure.Sharding;

namespace Tenants.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Tenants: DbContext, репозиторий, UoW, ShardResolver.
    /// </summary>
    public static IServiceCollection AddTenantsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'TENANTS_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<TenantsDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserTenantRepository, UserTenantRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IShardResolver, MySqlShardResolver>();

        return services;
    }
}

