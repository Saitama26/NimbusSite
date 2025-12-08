using Common.Application.Abstractions.Events;
using Common.Application.Abstractions.Messaging;
using Common.Infrastructure.Behaviors;
using Common.Infrastructure.Events;
using Common.Infrastructure.Messaging;
using MediatR;
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
    /// Добавить Common Infrastructure (EventBus, Sender, Behaviors)
    /// </summary>
    public static IServiceCollection AddCommonInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        // Добавляем MediatR
        services.AddMediatR(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
        });

        // Добавляем ISender через MediatR (используем полное имя чтобы избежать конфликта с MediatR.ISender)
        services.AddScoped<Common.Application.Abstractions.Messaging.ISender, MediatRSender>();

        // Добавляем Kafka EventBus
        services.AddKafkaEventBus(configuration);

        // Добавляем Behaviors (логирование, валидация, транзакции)
        services.AddBehaviors(assemblies);

        return services;
    }
}
