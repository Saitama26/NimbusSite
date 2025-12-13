using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Infrastructure.Events;
using Common.Infrastructure.Messaging;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Infrastructure слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Common Infrastructure (EventBus, Sender, Handlers, Validators)
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        // Регистрируем все handlers из assemblies
        foreach (var assembly in assemblies)
        {
            RegisterHandlers(services, assembly);
        }

        // Регистрируем FluentValidation валидаторы
        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        // Добавляем ISender (без MediatR)
        services.AddScoped<ISender, Sender>();

        // Добавляем Kafka EventBus
        services.AddKafkaEventBus(configuration);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                 i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                 i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<>) ||
                     i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
                .ToList();

            foreach (var interfaceType in interfaces)
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }
    }
}
