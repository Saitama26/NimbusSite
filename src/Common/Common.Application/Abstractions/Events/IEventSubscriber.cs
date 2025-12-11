using Common.Domain.Events;

namespace Common.Application.Abstractions.Events;

/// <summary>
/// Интерфейс для подписки на события из очереди сообщений (Kafka)
/// Используется модулями для обработки событий от других модулей
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Подписаться на событие типа TEvent
    /// </summary>
    /// <typeparam name="TEvent">Тип события</typeparam>
    /// <param name="handler">Обработчик события</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task SubscribeAsync<TEvent>(
        Func<TEvent, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Отписаться от события типа TEvent
    /// </summary>
    Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Начать прослушивание очереди событий
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Остановить прослушивание очереди событий
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}

