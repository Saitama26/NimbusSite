using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using AccessPermissions.Application.Mappings;
using Common.Application.Abstractions.Events;
using AccessPermissions.Application.EventHandlers;
using Contracts.Users.Events;
using Contracts.Projects.Events;
using Contracts.Tasks.Events;
using Contracts.Tenants.Events;

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

        // Event handlers
        services.AddScoped<IEventHandler<UserDeletedEvent>, UserDeletedEventHandler>();

        services.AddScoped<IEventHandler<ProjectDeletedEvent>, ProjectDeletedEventHandler>();

        services.AddScoped<IEventHandler<TaskDeletedEvent>, TaskDeletedEventHandler>();

        services.AddScoped<IEventHandler<TenantDeletedEvent>, TenantDeletedEventHandler>();

        return services;
    }
}

