using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Abstractions;
using Projects.Application.Abstractions.Views;
using Projects.Infrastructure;
using Projects.Infrastructure.EventHandlers;
using Projects.Infrastructure.Views.UsersViews;
using Tenants.Contracts.Events;
using Users.Contracts.Events;

namespace Projects.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Projects: DbContext, UoW, Views
    /// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
    /// Connection string определяется динамически через ShardResolver по TenantId из заголовка X-Tenant-Id
    /// </summary>
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (если еще не зарегистрированы)
        services.AddTenancy(configuration);

        // Регистрируем фабрику DbContext для динамического connection string
        services.AddScoped<IDbContextFactory<ProjectsDbContext>, TenantProjectsDbContextFactory>();

        // Регистрируем DbContext через фабрику
        services.AddScoped<ProjectsDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<ProjectsDbContext>>();
            return factory.CreateDbContext();
        });

        // Регистрируем IProjectsDbContext для использования в Application слое
        services.AddScoped<IProjectsDbContext>(sp => sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();

        // Регистрируем event handlers
        services.AddScoped<IEventHandler<UserDeletedEvent>, UserDeletedEventHandler>();
        services.AddScoped<IEventHandler<TenantDeletedEvent>, TenantDeletedEventHandler>();

        return services;
    }
}

