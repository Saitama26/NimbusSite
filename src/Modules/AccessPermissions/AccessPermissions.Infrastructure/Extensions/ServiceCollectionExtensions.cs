using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Infrastructure.Repositories;
using AccessPermissions.Infrastructure.UnitOfWork;

namespace AccessPermissions.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры AccessPermissions: DbContext, репозиторий, UoW
    /// </summary>
    public static IServiceCollection AddAccessPermissionsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("ACCESSPERMISSIONS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'ACCESSPERMISSIONS_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<AccessPermissionsDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IAccessPermissionRepository, AccessPermissionRepository>();
        services.AddScoped<IUnitOfWork, AccessPermissions.Infrastructure.UnitOfWork.UnitOfWork>();

        return services;
    }
}

