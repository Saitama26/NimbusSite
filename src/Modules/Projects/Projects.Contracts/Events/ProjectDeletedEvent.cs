using Common.Domain.Events;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления проекта (soft delete)
/// </summary>
public sealed class ProjectDeletedEvent : BaseIntegrationEvent
{
    public Guid ProjectId { get; }
    public int TenantId { get; }

    public ProjectDeletedEvent(
        Guid projectId,
        int tenantId,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
    }
}

