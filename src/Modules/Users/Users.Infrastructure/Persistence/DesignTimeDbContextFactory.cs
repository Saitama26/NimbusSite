using Common.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Users.Infrastructure.Persistence;

/// <summary>
/// Design-time фабрика для UsersDbContext (используется в миграциях).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        EnvLoader.Load(Path.Combine(basePath, "..", ".env"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine("src", "Modules", "Users", "Users.Api", "appsettings.json"), optional: true, reloadOnChange: false)
            .AddJsonFile(Path.Combine("src", "Modules", "Users", "Users.Api", "appsettings.Development.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            Environment.GetEnvironmentVariable("USERS_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string for Users is not configured (USERS_DB_CONNECTION_STRING or ConnectionStrings:DefaultConnection).");

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        var serverVersion = ServerVersion.Parse("8.0.0-mysql");
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new UsersDbContext(optionsBuilder.Options);
    }
}

