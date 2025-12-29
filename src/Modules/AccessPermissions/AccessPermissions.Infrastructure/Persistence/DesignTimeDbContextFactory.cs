using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Common.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AccessPermissions.Infrastructure.Persistence;

/// <summary>
/// Фабрика для создания DbContext в design-time (для миграций)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AccessPermissionsDbContext>
{
    public AccessPermissionsDbContext CreateDbContext(string[] args)
    {
        // Загружаем .env из корня проекта
        var solutionRoot = ProjectRootHelper.FindProjectRoot();
        if (solutionRoot != null)
        {
            var envPath = Path.Combine(solutionRoot, ".env");
            EnvLoader.Load(envPath);
        }

        // Путь к appsettings.json
        var solutionRootPath = solutionRoot ?? Directory.GetCurrentDirectory();
        var accessPermissionsApiPath = Path.Combine(solutionRootPath, "src", "Modules", "AccessPermissions", "AccessPermissions.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(accessPermissionsApiPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("ACCESSPERMISSIONS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "Connection string for AccessPermissions is not configured. " +
                "Please set ACCESSPERMISSIONS_DB_CONNECTION_STRING environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Checked .env file at: {(solutionRoot != null ? Path.Combine(solutionRoot, ".env") : ".env")}");

        var optionsBuilder = new DbContextOptionsBuilder<AccessPermissionsDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new AccessPermissionsDbContext(optionsBuilder.Options);
    }
}

