using Common.Domain.Events;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие удаления проекта.
/// </summary>
public sealed class ProjectDeletedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }

    public ProjectDeletedEvent(Guid projectId, Guid tenantId, DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
    }
}

