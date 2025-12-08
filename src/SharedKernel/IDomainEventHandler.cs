using MediatR;

namespace SharedKernel;

public interface IDomainEventHandler<T> : INotificationHandler<T> where T : IDomainEvent 
{
    Task<T> Handle(T domainEvent);
}