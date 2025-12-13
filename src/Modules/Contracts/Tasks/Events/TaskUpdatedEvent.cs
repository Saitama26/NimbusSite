using Common.Domain.Events;

namespace Contracts.Tasks.Events;

/// <summary>
/// Интеграционное событие обновления задачи
/// </summary>
public sealed class TaskUpdatedEvent : BaseDomainEvent
{
    public Guid TaskId { get; }
    public Guid TenantId { get; }
    public Guid ProjectId { get; }
    public string Title { get; }
    public string? Description { get; }
    public TaskPriorityContract Priority { get; }
    public DateTime? DueDate { get; }
    public DateTime UpdatedAt { get; }

    public TaskUpdatedEvent(
        Guid taskId,
        Guid tenantId,
        Guid projectId,
        string title,
        TaskPriorityContract priority,
        string? description = null,
        DateTime? dueDate = null,
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
        UpdatedAt = occurredAt ?? DateTime.UtcNow;
    }
}

