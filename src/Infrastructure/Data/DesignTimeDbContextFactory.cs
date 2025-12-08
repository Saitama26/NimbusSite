using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using DotNetEnv;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Infrastructure.Data;

/// <summary>
/// Factory для создания ApplicationDbContext во время разработки (для миграций)
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Читаем connection string из переменных окружения или .env файла
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // Пробуем найти .env файл в корне tempRep
            // Определяем путь к корню проекта (tempRep)
            var currentDir = Directory.GetCurrentDirectory();
            var infrastructureDir = Path.GetDirectoryName(typeof(DesignTimeDbContextFactory).Assembly.Location) 
                ?? currentDir;
            
            // Ищем корень tempRep (где находится .env файл)
            var possibleRoots = new[]
            {
                currentDir, // Текущая директория
                Path.Combine(currentDir, "..", "..", "..", ".."), // От Infrastructure/bin/Debug/net10.0
                Path.Combine(currentDir, "..", "..", ".."), // От Infrastructure
                Path.Combine(currentDir, "..", ".."), // От Infrastructure/Data
                Path.GetDirectoryName(infrastructureDir), // От Infrastructure/bin/Debug/net10.0
            };

            foreach (var root in possibleRoots)
            {
                if (string.IsNullOrEmpty(root)) continue;
                var envPath = Path.Combine(Path.GetFullPath(root), ".env");
                if (File.Exists(envPath))
                {
                    DotNetEnv.Env.Load(envPath);
                    connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        break;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "DATABASE_CONNECTION_STRING environment variable is not set. " +
                "Please set it in your .env file (should be in tempRep/.env) or environment variables.");
        }

        var serverVersion = ServerVersion.Parse(
            Environment.GetEnvironmentVariable("MYSQL_SERVER_VERSION") ?? "8.0.21");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, serverVersion);

        // Для DesignTime не нужны зависимости, создаем заглушки
        return new ApplicationDbContext(
            optionsBuilder.Options,
            null!, // IDomainEventDispatcher - не нужен для миграций
            null!  // ILogger - не нужен для миграций
        );
    }
}

