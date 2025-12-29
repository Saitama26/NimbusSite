using Common.Domain.Events;
using Projects.Contracts.Enums;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения статуса проекта
/// </summary>
public sealed class ProjectStatusChangedEvent : BaseIntegrationEvent
{
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public ProjectStatusContract OldStatus { get; }
    public ProjectStatusContract NewStatus { get; }

    public ProjectStatusChangedEvent(
        Guid projectId,
        int tenantId,
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

