using SharedKernel;

namespace Domain.Users.Events;

public sealed record UserDeactivatedEvent(Guid userId) : IDomainEvent { }