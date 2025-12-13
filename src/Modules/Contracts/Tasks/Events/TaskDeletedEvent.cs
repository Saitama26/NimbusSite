using Common.Domain.Events;

namespace Contracts.Tasks.Events;

/// <summary>
/// Интеграционное событие удаления задачи
/// </summary>
public sealed class TaskDeletedEvent : BaseDomainEvent
{
    public Guid TaskId { get; }
    public Guid TenantId { get; }
    public Guid ProjectId { get; }
    public DateTime DeletedAt { get; }

    public TaskDeletedEvent(
        Guid taskId,
        Guid tenantId,
        Guid projectId,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        DeletedAt = occurredAt ?? DateTime.UtcNow;
    }
}

