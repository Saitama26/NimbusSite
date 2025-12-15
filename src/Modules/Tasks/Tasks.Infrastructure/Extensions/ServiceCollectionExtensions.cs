using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Application.Abstractions;
using Tasks.Application.Abstractions.Views;
using Tasks.Infrastructure.Repositories;
using Tasks.Infrastructure.Sharding;
using Tasks.Infrastructure.Views.ProjectsViews;
using Tasks.Infrastructure.Views.TenantsViews;
using Tasks.Infrastructure.Views.UsersViews;

namespace Tasks.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Tasks: DbContext, репозиторий, UoW, ShardResolver
    /// </summary>
    public static IServiceCollection AddTasksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("TASKS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'TASKS_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<TasksDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<IProjectViewRepository, ProjectViewRepository>();
        services.AddScoped<ITenantViewRepository, TenantViewRepository>();
        services.AddScoped<IUnitOfWork, Tasks.Infrastructure.UnitOfWork.UnitOfWork>();
        services.AddScoped<IShardResolver>(sp => new MySqlShardResolver(
            sp.GetRequiredService<TasksDbContext>(),
            connectionString));

        return services;
    }
}

