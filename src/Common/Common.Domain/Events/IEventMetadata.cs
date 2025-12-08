namespace Common.Domain.Events;

/// <summary>
/// Метаданные события для публикации в очередь (RabbitMQ/Kafka)
/// </summary>
public interface IEventMetadata
{
    /// <summary>
    /// Тип события (полное имя типа)
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Версия события (для версионирования схем)
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Время создания события
    /// </summary>
    DateTime Timestamp { get; }

    /// <summary>
    /// Идентификатор для отслеживания цепочки событий/запросов
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Идентификатор тенанта (если применимо)
    /// </summary>
    Guid? TenantId { get; }
}

