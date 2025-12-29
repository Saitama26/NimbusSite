using Common.Domain.Events;

namespace Common.Application.Abstractions.Events;

/// <summary>
/// Интерфейс для публикации интеграционных событий в очередь сообщений (Kafka)
/// Используется для асинхронного взаимодействия между модулями
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Опубликовать одно интеграционное событие в очередь
    /// </summary>
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;

    /// <summary>
    /// Опубликовать несколько интеграционных событий в очередь
    /// </summary>
    Task PublishAsync(IEnumerable<IIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default);
}

