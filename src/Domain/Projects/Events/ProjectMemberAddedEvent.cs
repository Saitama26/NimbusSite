using SharedKernel;

namespace Domain.Projects.Events;

public sealed record ProjectMemberAddedEvent(Guid projectId, Guid userId) : IDomainEvent { }

