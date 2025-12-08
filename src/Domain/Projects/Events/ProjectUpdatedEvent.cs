using SharedKernel;

namespace Domain.Projects.Events;

public sealed record ProjectUpdatedEvent(Guid projectId, string name) : IDomainEvent { }

