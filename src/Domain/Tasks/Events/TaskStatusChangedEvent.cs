using Domain.Tasks.ValueObjects;
using SharedKernel;

namespace Domain.Tasks.Events;

public sealed record TaskStatusChangedEvent(Guid taskId,
    Status oldStatus, Status newStatus) : IDomainEvent { }