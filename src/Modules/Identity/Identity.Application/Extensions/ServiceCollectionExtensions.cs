using AutoMapper;
using Common.Application.Abstractions.Events;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Mappings;
using Identity.Application.EventHandlers;
using System.Reflection;

namespace Identity.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Identity Application сервисы
    /// </summary>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // Регистрируем AutoMapper
        services.AddAutoMapper(typeof(IdentityMappingProfile));

        // Регистрируем все обработчики событий
        RegisterEventHandlers(services, typeof(IdentityMappingProfile).Assembly);

        return services;
    }

    private static void RegisterEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}

