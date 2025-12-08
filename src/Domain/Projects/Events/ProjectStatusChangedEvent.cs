using Domain.Projects.ValueObjects;
using SharedKernel;

namespace Domain.Projects.Events;

public sealed record ProjectStatusChangedEvent(Guid projectId, 
    ProjectStatus oldStatus, ProjectStatus newStatus) : IDomainEvent { }