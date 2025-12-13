using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using AccessPermissions.Application.Mappings;

namespace AccessPermissions.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить AccessPermissions Application сервисы
    /// </summary>
    public static IServiceCollection AddAccessPermissionsApplication(this IServiceCollection services)
    {
        // Регистрируем AutoMapper
        services.AddAutoMapper(typeof(AccessPermissionMappingProfile));

        return services;
    }
}

