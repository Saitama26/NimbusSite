using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Projects.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для ProjectsDbContext (используется в миграциях).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProjectsDbContext>
{
    public ProjectsDbContext CreateDbContext(string[] args)
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
        var projectsApiPath = Path.Combine(solutionRootPath, "src", "Modules", "Projects", "Projects.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectsApiPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("PROJECTS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "Connection string for Projects is not configured. " +
                "Please set PROJECTS_DB_CONNECTION_STRING environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Checked .env file at: {(solutionRoot != null ? Path.Combine(solutionRoot, ".env") : ".env")}");

        var optionsBuilder = new DbContextOptionsBuilder<ProjectsDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ProjectsDbContext(optionsBuilder.Options);
    }
}

