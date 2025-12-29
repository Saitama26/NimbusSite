namespace Common.Domain.Events;

/// <summary>
/// Базовый интерфейс для всех интеграционных событий
/// Используется для асинхронного взаимодействия между модулями через Kafka
/// </summary>
public interface IIntegrationEvent
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

