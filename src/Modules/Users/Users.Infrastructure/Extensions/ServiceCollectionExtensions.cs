using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Abstractions;

namespace Users.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Users: DbContext, UoW
    /// Connection string определяется динамически по TenantId через TenantConnectionCache
    /// </summary>
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (TenantConnectionCache, UserTenantService)
        services.AddTenancy(configuration);

        // Регистрируем фабрику DbContext
        services.AddScoped<IDbContextFactory<UsersDbContext>, TenantUsersDbContextFactory>();

        // Регистрируем DbContext через фабрику
        services.AddScoped<UsersDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<UsersDbContext>>();
            return factory.CreateDbContext();
        });

        // Регистрируем IUsersDbContext
        services.AddScoped<IUsersDbContext>(sp => sp.GetRequiredService<UsersDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
