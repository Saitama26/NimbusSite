using Common.Domain.Events;

namespace Common.Application.Abstractions.Events;

/// <summary>
/// Интерфейс обработчика доменного события
/// </summary>
/// <typeparam name="TEvent">Тип события</typeparam>
public interface IEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    /// <summary>
    /// Обработать событие
    /// </summary>
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken = default);
}

