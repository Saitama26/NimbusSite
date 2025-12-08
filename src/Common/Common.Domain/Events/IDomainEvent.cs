namespace Common.Domain.Events;

/// <summary>
/// Базовый интерфейс для всех доменных событий
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Время создания события
    /// </summary>
    DateTime OccurredOn { get; }
}

