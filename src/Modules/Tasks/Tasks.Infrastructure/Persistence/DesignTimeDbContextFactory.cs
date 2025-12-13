using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Tasks.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для TasksDbContext (используется в миграциях)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TasksDbContext>
{
    public TasksDbContext CreateDbContext(string[] args)
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
        var tasksApiPath = Path.Combine(solutionRootPath, "src", "Modules", "Tasks", "Tasks.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(tasksApiPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("TASKS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException(
                "Connection string for Tasks is not configured. " +
                "Please set TASKS_DB_CONNECTION_STRING environment variable or configure ConnectionStrings:DefaultConnection in appsettings.json. " +
                $"Checked .env file at: {(solutionRoot != null ? Path.Combine(solutionRoot, ".env") : ".env")}");

        var optionsBuilder = new DbContextOptionsBuilder<TasksDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new TasksDbContext(optionsBuilder.Options);
    }
}

