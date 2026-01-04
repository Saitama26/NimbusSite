using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Identity.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для IdentityDbContext (используется в миграциях)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
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
        var identityApiPath = Path.Combine(solutionRootPath, "src", "Modules", "Identity", "Identity.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(identityApiPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "Connection string for Identity is not configured. " +
                "Please set IDENTITY_DB_CONNECTION_STRING environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Checked .env file at: {(solutionRoot != null ? Path.Combine(solutionRoot, ".env") : ".env")}");

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new IdentityDbContext(optionsBuilder.Options);
    }
}

