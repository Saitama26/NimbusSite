using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tenants.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для создания TenantsDbContext при выполнении миграций.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantsDbContext>
{
    public TenantsDbContext CreateDbContext(string[] args)
    {
        // Пытаемся найти корень решения несколькими способами
        var basePath = GetSolutionRoot() 
            ?? GetSolutionRootFromAssembly() 
            ?? Directory.GetCurrentDirectory();

        // Пробуем загрузить .env/.env.template (EnvLoader сам обработает отсутствие файла)
        EnvLoader.Load();

        var tenantsApiPath = Path.Combine(basePath, "src", "Modules", "Tenants", "Tenants.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(Path.Combine(basePath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(tenantsApiPath, "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine(tenantsApiPath, "appsettings.Development.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        // Пробуем получить строку подключения из разных источников
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? configuration["TENANTS_DB_CONNECTION_STRING"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var searchedPaths = new[]
            {
                Path.Combine(basePath, "appsettings.json"),
                Path.Combine(tenantsApiPath, "appsettings.json"),
                Path.Combine(tenantsApiPath, "appsettings.Development.json"),
                Path.Combine(basePath, ".env")
            };

            var existingFiles = searchedPaths.Where(p => File.Exists(p)).ToList();
            var missingFiles = searchedPaths.Where(p => !File.Exists(p)).ToList();

            // Проверяем, что загружено из конфигурации (для отладки)
            var configValues = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = configuration["ConnectionStrings:DefaultConnection"],
                ["DefaultConnection"] = configuration.GetConnectionString("DefaultConnection"),
                ["TENANTS_DB_CONNECTION_STRING"] = configuration["TENANTS_DB_CONNECTION_STRING"]
            };

            var envValues = new Dictionary<string, string?>
            {
                ["ConnectionStrings__DefaultConnection"] = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
                ["TENANTS_DB_CONNECTION_STRING"] = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING")
            };

            // Форматируем значения для отображения (скрываем чувствительные данные)
            string FormatValue(string? value)
            {
                if (value == null) return "(null)";
                if (string.IsNullOrWhiteSpace(value)) return "(empty)";
                // Показываем первые 50 символов для отладки
                var preview = value.Length > 50 ? value.Substring(0, 50) + "..." : value;
                return $"\"{preview}\"";
            }

            var errorMessage = "Connection string not found or is empty. Please configure 'DefaultConnection' or 'TENANTS_DB_CONNECTION_STRING' " +
                "in appsettings.json, environment variables, or .env file with a non-empty value.\n\n" +
                $"Base path used: {basePath}\n" +
                $"Current directory: {Directory.GetCurrentDirectory()}\n" +
                "\nSearched configuration files:\n" +
                (existingFiles.Any() 
                    ? $"Found: {string.Join("\n  ", existingFiles)}\n" 
                    : "None found\n") +
                (missingFiles.Any() 
                    ? $"Missing: {string.Join("\n  ", missingFiles)}\n" 
                    : "") +
                "\nConfiguration values checked:\n" +
                string.Join("\n", configValues.Select(kv => $"  {kv.Key} = {FormatValue(kv.Value)}")) +
                "\n\nEnvironment variables checked:\n" +
                string.Join("\n", envValues.Select(kv => $"  {kv.Key} = {FormatValue(kv.Value)}")) +
                "\n\nNOTE: If TENANTS_DB_CONNECTION_STRING shows a value but connection string is still empty, " +
                "check that the .env file contains a non-empty value for this variable. " +
                "The value should be in format: TENANTS_DB_CONNECTION_STRING=Server=...;Database=...;User=...;Password=...;";

            throw new InvalidOperationException(errorMessage);
        }

        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();
        // Используем явную версию MySQL вместо AutoDetect для design-time (миграции)
        // AutoDetect требует активного подключения к БД, что может быть проблемой в design-time
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new TenantsDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Пытается найти корень решения, поднимаясь вверх, пока не встретит .sln или .env(.template).
    /// </summary>
    private static string? GetSolutionRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var hasSln = dir.GetFiles("*.sln").Any();
            var hasEnv = dir.GetFiles(".env").Any() || dir.GetFiles(".env.template").Any();
            if (hasSln || hasEnv)
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// Пытается найти корень решения, используя путь к сборке.
    /// </summary>
    private static string? GetSolutionRootFromAssembly()
    {
        try
        {
            var assemblyLocation = typeof(DesignTimeDbContextFactory).Assembly.Location;
            if (string.IsNullOrEmpty(assemblyLocation))
                return null;

            var assemblyDir = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)!);
            while (assemblyDir != null)
            {
                var hasSln = assemblyDir.GetFiles("*.sln").Any();
                if (hasSln)
                {
                    return assemblyDir.FullName;
                }
                assemblyDir = assemblyDir.Parent;
            }
        }
        catch
        {
            // Игнорируем ошибки при определении пути к сборке
        }
        return null;
    }
}

