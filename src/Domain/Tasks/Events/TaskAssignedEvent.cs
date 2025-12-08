using SharedKernel;

namespace Domain.Tasks.Events;

public sealed record TaskAssignedEvent(Guid taskId, Guid userId) : IDomainEvent { }