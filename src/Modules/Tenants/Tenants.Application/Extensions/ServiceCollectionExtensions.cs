using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Tenants.Application.Mappings;

namespace Tenants.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Tenants Application сервисы
    /// </summary>
    public static IServiceCollection AddTenantsApplication(this IServiceCollection services)
    {
        // Регистрируем AutoMapper
        services.AddAutoMapper(typeof(TenantMappingProfile));

        // TODO: Зарегистрировать:
        // - Репозитории (ITenantRepository) - будет в Infrastructure
        // - Handlers регистрируются автоматически через MediatR
        // - Validators регистрируются автоматически через FluentValidation
        // - UnitOfWork - будет в Infrastructure

        return services;
    }
}

