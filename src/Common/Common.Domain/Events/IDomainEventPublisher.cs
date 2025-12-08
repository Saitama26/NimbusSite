namespace Common.Domain.Events;

/// <summary>
/// Интерфейс для сущностей, которые публикуют доменные события
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Список доменных событий
    /// </summary>
    IReadOnlyList<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Добавить доменное событие
    /// </summary>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Удалить доменное событие
    /// </summary>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Очистить все доменные события (после публикации)
    /// </summary>
    void ClearDomainEvents();
}

