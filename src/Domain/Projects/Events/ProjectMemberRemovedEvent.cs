using SharedKernel;

namespace Domain.Projects.Events;

public sealed record ProjectMemberRemovedEvent(Guid projectId, Guid userId) : IDomainEvent { }

