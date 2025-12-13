using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Common.Infrastructure.Configuration;

namespace AccessPermissions.Infrastructure.Persistence;

/// <summary>
/// Фабрика для создания DbContext в design-time (для миграций)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccessPermissionsDbContext>
{
    public AccessPermissionsDbContext CreateDbContext(string[] args)
    {
        // Загружаем .env из корня проекта
        ProjectRootHelper.LoadEnvFromProjectRoot();

        var connectionString = Environment.GetEnvironmentVariable("ACCESSPERMISSIONS_DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("ACCESSPERMISSIONS_DB_CONNECTION_STRING environment variable is not set.");

        var optionsBuilder = new DbContextOptionsBuilder<AccessPermissionsDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new AccessPermissionsDbContext(optionsBuilder.Options);
    }
}

