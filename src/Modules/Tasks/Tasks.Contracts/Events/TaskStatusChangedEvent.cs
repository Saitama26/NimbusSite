using Common.Domain.Events;
using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения статуса задачи
/// </summary>
public sealed class TaskStatusChangedEvent : BaseIntegrationEvent
{
    public Guid TaskId { get; }
    public int TenantId { get; }
    public Guid ProjectId { get; }
    public TaskStatusContract OldStatus { get; }
    public TaskStatusContract NewStatus { get; }
    public DateTime? StartedAt { get; }
    public DateTime? CompletedAt { get; }
    public DateTime? ChangedAt { get; }

    public TaskStatusChangedEvent(
        Guid taskId,
        int tenantId,
        Guid projectId,
        TaskStatusContract oldStatus,
        TaskStatusContract newStatus,
        DateTime? startedAt = null,
        DateTime? completedAt = null,
        DateTime? changedAt = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        ChangedAt = changedAt;
    }
}

