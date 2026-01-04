using Microsoft.Extensions.DependencyInjection;

namespace Users.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Users Application сервисы
    /// Handlers и Validators регистрируются автоматически через Common.Infrastructure
    /// </summary>
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        // Handlers регистрируются автоматически через Common.Infrastructure.AddCommonInfrastructure
        // Validators регистрируются автоматически через FluentValidation
        // IUsersDbContext регистрируется в Infrastructure

        return services;
    }
}

