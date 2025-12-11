using Common.Domain.Events;
using Projects.Domain.Enums;

namespace Projects.Domain.Events;

/// <summary>
/// Интеграционное событие изменения статуса проекта.
/// </summary>
public sealed class ProjectStatusChangedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public ProjectStatus OldStatus { get; }
    public ProjectStatus NewStatus { get; }

    public ProjectStatusChangedEvent(
        Guid projectId,
        Guid tenantId,
        ProjectStatus oldStatus,
        ProjectStatus newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

