using Domain.Users.ValueObjects;
using SharedKernel;

namespace Domain.Users.Events;

public sealed record UserRoleChangedEvent(Guid userId, UserRole oldRole, UserRole newRole) : IDomainEvent { }