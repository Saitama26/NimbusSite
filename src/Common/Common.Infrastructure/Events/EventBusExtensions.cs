using Common.Application.Abstractions.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Events;

/// <summary>
/// Расширения для регистрации EventBus в DI
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Добавить Kafka EventBus и Subscriber
    /// </summary>
    public static IServiceCollection AddKafkaEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventBus, KafkaEventBus>();
        services.AddSingleton<KafkaEventSubscriber>();
        services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<KafkaEventSubscriber>());
        services.AddHostedService(sp => sp.GetRequiredService<KafkaEventSubscriber>());
        
        return services;
    }
}
