using Common.Domain.Events;

namespace Common.Application.Abstractions.Events;

/// <summary>
/// Интерфейс для публикации доменных событий в очередь сообщений (RabbitMQ/Kafka)
/// Используется для асинхронного взаимодействия между модулями
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Опубликовать одно событие в очередь
    /// </summary>
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Опубликовать несколько событий в очередь
    /// </summary>
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

