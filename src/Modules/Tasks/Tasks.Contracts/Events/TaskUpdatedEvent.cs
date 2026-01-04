using Common.Domain.Events;
using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления задачи
/// </summary>
public sealed class TaskUpdatedEvent : BaseIntegrationEvent
{
    public Guid TaskId { get; }
    public int TenantId { get; }
    public Guid ProjectId { get; }
    public string? Title { get; }
    public string? Description { get; }
    public TaskPriorityContract? Priority { get; }
    public DateTime? DueDate { get; }
    public DateTime? UpdatedAt { get; }

    public TaskUpdatedEvent(
        Guid taskId,
        int tenantId,
        Guid projectId,
        string? title = null,
        string? description = null,
        TaskPriorityContract? priority = null,
        DateTime? dueDate = null,
        DateTime? updatedAt = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = updatedAt;
    }
}

