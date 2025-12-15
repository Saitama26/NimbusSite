using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Abstractions;
using Projects.Application.Abstractions.Views;
using Projects.Infrastructure.Repositories;
using Projects.Infrastructure.Sharding;
using Projects.Infrastructure.Views.UsersViews;

namespace Projects.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Projects: DbContext, репозиторий, UoW.
    /// </summary>
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("PROJECTS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'PROJECTS_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<ProjectsDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectUserRepository, ProjectUserRepository>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<IUnitOfWork, Projects.Infrastructure.UnitOfWork>();
        services.AddScoped<IShardResolver>(sp => new MySqlShardResolver(
            sp.GetRequiredService<ProjectsDbContext>(),
            connectionString));

        return services;
    }
}

