using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Application.Abstractions.Views;
using AccessPermissions.Infrastructure;
using AccessPermissions.Infrastructure.EventHandlers;
using AccessPermissions.Infrastructure.UnitOfWork;
using AccessPermissions.Infrastructure.Views.ProjectsViews;
using AccessPermissions.Infrastructure.Views.TasksViews;
using AccessPermissions.Infrastructure.Views.TenantsViews;
using AccessPermissions.Infrastructure.Views.UsersViews;
using Projects.Contracts.Events;
using Tasks.Contracts.Events;
using Tenants.Contracts.Events;
using Users.Contracts.Events;

namespace AccessPermissions.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры AccessPermissions: DbContext, UoW, Views
    /// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
    /// Connection string определяется динамически через ShardResolver по TenantId из заголовка X-Tenant-Id
    /// </summary>
    public static IServiceCollection AddAccessPermissionsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (если еще не зарегистрированы)
        services.AddTenancy(configuration);

        // Регистрируем фабрику DbContext для динамического connection string
        services.AddScoped<IDbContextFactory<AccessPermissionsDbContext>, TenantAccessPermissionsDbContextFactory>();

        // Регистрируем DbContext через фабрику
        services.AddScoped<AccessPermissionsDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<AccessPermissionsDbContext>>();
            return factory.CreateDbContext();
        });

        // Регистрируем IAccessPermissionsDbContext для использования в Application слое
        services.AddScoped<IAccessPermissionsDbContext>(sp => sp.GetRequiredService<AccessPermissionsDbContext>());

        services.AddScoped<IUnitOfWork, AccessPermissions.Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<IProjectViewRepository, ProjectViewRepository>();
        services.AddScoped<ITaskViewRepository, TaskViewRepository>();
        services.AddScoped<ITenantViewRepository, TenantViewRepository>();

        // Регистрируем event handlers
        services.AddScoped<IEventHandler<UserDeletedEvent>, UserDeletedEventHandler>();
        services.AddScoped<IEventHandler<ProjectDeletedEvent>, ProjectDeletedEventHandler>();
        services.AddScoped<IEventHandler<TaskDeletedEvent>, TaskDeletedEventHandler>();
        services.AddScoped<IEventHandler<TenantDeletedEvent>, TenantDeletedEventHandler>();

        return services;
    }
}

