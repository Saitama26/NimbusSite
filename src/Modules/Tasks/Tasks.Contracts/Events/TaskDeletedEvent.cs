using Common.Domain.Events;

namespace Tasks.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления задачи (soft delete)
/// </summary>
public sealed class TaskDeletedEvent : BaseIntegrationEvent
{
    public Guid TaskId { get; }
    public int TenantId { get; }
    public Guid ProjectId { get; }
    public DateTime? DeletedAt { get; }

    public TaskDeletedEvent(
        Guid taskId,
        int tenantId,
        Guid projectId,
        DateTime? deletedAt = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        DeletedAt = deletedAt;
    }
}

