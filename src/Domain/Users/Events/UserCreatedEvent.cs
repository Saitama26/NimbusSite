using SharedKernel;

namespace Domain.Users.Events;

public sealed record UserCreatedEvent(Guid userId, string userName, string email) : IDomainEvent { }
