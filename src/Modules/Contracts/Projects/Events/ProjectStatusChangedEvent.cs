using Common.Domain.Events;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие изменения статуса проекта.
/// </summary>
public sealed class ProjectStatusChangedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public ProjectStatusContract OldStatus { get; }
    public ProjectStatusContract NewStatus { get; }

    public ProjectStatusChangedEvent(
        Guid projectId,
        Guid tenantId,
        ProjectStatusContract oldStatus,
        ProjectStatusContract newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

