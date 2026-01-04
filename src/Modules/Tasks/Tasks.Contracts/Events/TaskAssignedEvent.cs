using Common.Domain.Events;

namespace Tasks.Contracts.Events;

/// <summary>
/// Интеграционное событие назначения задачи пользователю
/// </summary>
public sealed class TaskAssignedEvent : BaseIntegrationEvent
{
    public Guid TaskId { get; }
    public int TenantId { get; }
    public Guid ProjectId { get; }
    public Guid? AssignedToUserId { get; }
    public Guid? PreviousUserId { get; }
    public DateTime? AssignedAt { get; }

    public TaskAssignedEvent(
        Guid taskId,
        int tenantId,
        Guid projectId,
        Guid? assignedToUserId,
        Guid? previousUserId = null,
        DateTime? assignedAt = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        AssignedToUserId = assignedToUserId;
        PreviousUserId = previousUserId;
        AssignedAt = assignedAt;
    }
}

