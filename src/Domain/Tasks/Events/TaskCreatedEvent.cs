using SharedKernel; 

namespace Domain.Tasks.Events;

public sealed record TaskCreatedEvent(Guid taskId, string title) : IDomainEvent { }