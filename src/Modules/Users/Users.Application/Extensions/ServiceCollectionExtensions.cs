using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Users.Application.Mappings;

namespace Users.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Users Application сервисы
    /// </summary>
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        // Регистрируем AutoMapper
        services.AddAutoMapper(typeof(UserMappingProfile));

        // TODO: Зарегистрировать:
        // - Репозитории (IUserRepository) - будет в Infrastructure
        // - Handlers регистрируются автоматически через MediatR
        // - Validators регистрируются автоматически через FluentValidation
        // - UnitOfWork - будет в Infrastructure

        return services;
    }
}

