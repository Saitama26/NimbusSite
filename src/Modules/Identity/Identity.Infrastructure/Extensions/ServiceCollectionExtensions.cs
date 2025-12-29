using Common.Application.Abstractions;
using Common.Application.Abstractions.Events;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Views;
using Identity.Infrastructure;
using Identity.Infrastructure.EventHandlers;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Views.TenantsViews;
using Identity.Infrastructure.Views.UsersViews;
using Users.Contracts.Events;

namespace Identity.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация инфраструктуры Identity: DbContext, UoW, Views, сервисы
    /// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
    /// Connection string определяется динамически через ShardResolver по TenantId из заголовка X-Tenant-Id
    /// </summary>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрируем Tenancy сервисы (если еще не зарегистрированы)
        services.AddTenancy(configuration);

        // Регистрируем фабрику DbContext для динамического connection string
        services.AddScoped<IDbContextFactory<IdentityDbContext>, TenantIdentityDbContextFactory>();

        // Регистрируем DbContext через фабрику
        services.AddScoped<IdentityDbContext>(sp =>
        {
            var factory = sp.GetRequiredService<IDbContextFactory<IdentityDbContext>>();
            return factory.CreateDbContext();
        });

        // Регистрируем IIdentityDbContext для использования в Application слое
        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        services.AddScoped<IUserViewRepository, UserViewRepository>();
        services.AddScoped<ITenantViewRepository, TenantViewRepository>();

        // Регистрация сервисов
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenHasher, TokenHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Регистрируем event handlers
        services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedEventHandler>();
        services.AddScoped<IEventHandler<UserDeletedEvent>, UserDeletedEventHandler>();
        services.AddScoped<IEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IEventHandler<UserStatusChangedEvent>, UserStatusChangedEventHandler>();
        services.AddScoped<IEventHandler<UserUpdatedEvent>, UserUpdatedEventHandler>();

        return services;
    }
}

