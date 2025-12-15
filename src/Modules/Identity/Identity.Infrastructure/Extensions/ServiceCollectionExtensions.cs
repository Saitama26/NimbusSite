using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Views;
using Identity.Infrastructure.Repositories;
using Identity.Infrastructure.Sharding;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Views.TenantsViews;
using Identity.Infrastructure.Views.UsersViews;

namespace Identity.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Identity: DbContext, репозитории, UoW, ShardResolver, сервисы
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Try environment variable first, then configuration
        var connectionString = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' or 'IDENTITY_DB_CONNECTION_STRING' is not configured.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUserCredentialsRepository, UserCredentialsRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<ITenantViewRepository, TenantViewRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IShardResolver>(sp => new MySqlShardResolver(
            sp.GetRequiredService<IdentityDbContext>(),
            connectionString));

        // Регистрация сервисов
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenHasher, TokenHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}

