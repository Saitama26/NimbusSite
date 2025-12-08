using Domain.Users.ValueObjects;
using SharedKernel;

namespace Domain.Access.Events;

public sealed record PermissionGrantedEvent(Guid userId, Guid projectId, UserRole role) : IDomainEvent { }