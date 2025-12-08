using SharedKernel;

namespace Domain.Access.Events;

public sealed record PermissionRevokedEvent(Guid userId, Guid projectId) : IDomainEvent { }