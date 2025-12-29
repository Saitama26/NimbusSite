using Common.Domain.Events;

namespace Common.Application.Abstractions.Events;

/// <summary>
/// Интерфейс обработчика интеграционного события
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : IIntegrationEvent
{
    /// <summary>
    /// Обработать интеграционное событие
    /// </summary>
    Task Handle(TEvent integrationEvent, CancellationToken cancellationToken = default);
}

