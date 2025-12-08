namespace Common.Infrastructure.Events.Models;

/// <summary>
/// Обертка события для публикации в очередь (RabbitMQ/Kafka)
/// </summary>
public sealed class EventEnvelope
{
    /// <summary>
    /// Тип события (полное имя типа)
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Версия события (для версионирования схем)
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Время создания события
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Идентификатор для отслеживания цепочки событий/запросов
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Идентификатор тенанта (если применимо)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Сериализованное событие (JSON)
    /// </summary>
    public string Payload { get; set; } = string.Empty;
}
