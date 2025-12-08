namespace SharedKernel;

public interface IDomainEventPublisher
{
    IReadOnlyList<IDomainEvent> Events { get; }

    void AddEvent(IDomainEvent eventItem);
    void RemoveEvent(IDomainEvent eventItem);
    void ClearDomainEvents();
}