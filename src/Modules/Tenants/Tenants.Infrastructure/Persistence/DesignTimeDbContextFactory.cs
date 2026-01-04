using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Tenants.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для создания TenantsDbContext при выполнении миграций.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantsDbContext>
{
    public TenantsDbContext CreateDbContext(string[] args)
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
        var tenantsApiPath = Path.Combine(solutionRootPath, "src", "Modules", "Tenants", "Tenants.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(tenantsApiPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Пробуем получить строку подключения из разных источников
        var connectionString =
            Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING") 
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? configuration["TENANTS_DB_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string for Tenants is not configured. " +
                "Please set TENANTS_DB_CONNECTION_STRING environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Checked .env file at: {(solutionRoot != null ? Path.Combine(solutionRoot, ".env") : ".env")}");

        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new TenantsDbContext(optionsBuilder.Options);
    }
}