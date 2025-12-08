namespace Common.Domain.Events;

/// <summary>
/// Базовая реализация доменного события
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }

    protected BaseDomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    protected BaseDomainEvent(Guid eventId, DateTime occurredOn)
    {
        EventId = eventId;
        OccurredOn = occurredOn;
    }
}

