using SharedKernel;

namespace Domain.Projects.Events;

public sealed record ProjectCreatedEvent(Guid projectId, string name) : IDomainEvent { }