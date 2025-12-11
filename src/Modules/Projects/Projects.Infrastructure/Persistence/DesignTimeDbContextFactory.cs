using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Projects.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для ProjectsDbContext (используется в миграциях).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProjectsDbContext>
{
    public ProjectsDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        EnvLoader.Load(Path.Combine(basePath, "..", ".env"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine("src", "Modules", "Projects", "Projects.Api", "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine("src", "Modules", "Projects", "Projects.Api", "appsettings.Development.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("PROJECTS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string for Projects is not configured (PROJECTS_DB_CONNECTION_STRING or ConnectionStrings:DefaultConnection).");

        var optionsBuilder = new DbContextOptionsBuilder<ProjectsDbContext>();
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ProjectsDbContext(optionsBuilder.Options);
    }
}

