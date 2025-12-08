namespace SharedKernel;

public abstract class DomainEventEntity : IDomainEventPublisher
{
    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
    
    public IReadOnlyList<IDomainEvent> Events => _domainEvents.AsReadOnly();

    public void AddEvent(IDomainEvent eventItem)
    {
        if (eventItem is null)
        {
            throw new ArgumentNullException(nameof(eventItem));
        }
        _domainEvents.Add(eventItem);
    }

    public void RemoveEvent(IDomainEvent eventItem)
    {
        if (eventItem is null)
        {
            throw new ArgumentNullException(nameof(eventItem));
        }
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}