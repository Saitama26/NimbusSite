using Common.Application.Abstractions;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Application.Abstractions;
using Tasks.Application.Abstractions.Views;
using Tasks.Infrastructure;
using Tasks.Infrastructure.Views.ProjectsViews;
using Tasks.Infrastructure.Views.TenantsViews;
using Tasks.Infrastructure.Views.UsersViews;

namespace Tasks.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Tasks: DbContext, UoW, Views
    /// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
    /// Connection string определяется динамически через ShardResolver по TenantId из заголовка X-Tenant-Id
    /// </summary>
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (если еще не зарегистрированы)
        services.AddTenancy(configuration);

        // Регистрируем фабрику DbContext для динамического connection string
        services.AddScoped<IDbContextFactory<TasksDbContext>, TenantTasksDbContextFactory>();

        // Регистрируем DbContext через фабрику
        services.AddScoped<TasksDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<TasksDbContext>>();
            return factory.CreateDbContext();
        });

        // Регистрируем ITasksDbContext для использования в Application слое
        services.AddScoped<ITasksDbContext>(sp => sp.GetRequiredService<TasksDbContext>());

        services.AddScoped<IUnitOfWork, Tasks.Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<IProjectViewRepository, ProjectViewRepository>();
        services.AddScoped<ITenantViewRepository, TenantViewRepository>();

        return services;
    }
}

