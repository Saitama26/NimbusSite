namespace Common.Domain.Events;

/// <summary>
/// Базовая реализация интеграционного события
/// Используется для асинхронного взаимодействия между модулями через Kafka
/// </summary>
public abstract class BaseIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; }
    public DateTime OccurredOn { get; init; }

    protected BaseIntegrationEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    protected BaseIntegrationEvent(Guid eventId, DateTime occurredOn)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
    }
}

