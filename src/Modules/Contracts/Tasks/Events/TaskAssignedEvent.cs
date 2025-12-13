using Common.Domain.Events;

namespace Contracts.Tasks.Events;

/// <summary>
/// Интеграционное событие назначения задачи пользователю
/// </summary>
public sealed class TaskAssignedEvent : BaseDomainEvent
{
    public Guid TaskId { get; }
    public Guid TenantId { get; }
    public Guid ProjectId { get; }
    public Guid? PreviousUserId { get; }
    public Guid? NewUserId { get; }
    public DateTime AssignedAt { get; }

    public TaskAssignedEvent(
        Guid taskId,
        Guid tenantId,
        Guid projectId,
        Guid? newUserId,
        Guid? previousUserId = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        PreviousUserId = previousUserId;
        NewUserId = newUserId;
        AssignedAt = occurredAt ?? DateTime.UtcNow;
    }
}

