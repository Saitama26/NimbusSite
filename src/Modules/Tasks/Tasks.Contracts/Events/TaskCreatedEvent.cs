using Common.Domain.Events;
using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Events;

/// <summary>
/// Интеграционное событие создания задачи
/// Публикуется через Kafka для других модулей
/// </summary>
public sealed class TaskCreatedEvent : BaseIntegrationEvent
{
    public Guid TaskId { get; }
    public int TenantId { get; }
    public Guid ProjectId { get; }
    public string Title { get; }
    public string? Description { get; }
    public TaskStatusContract Status { get; }
    public TaskPriorityContract Priority { get; }
    public Guid? AssignedToUserId { get; }
    public Guid CreatedByUserId { get; }
    public DateTime? DueDate { get; }
    public DateTime CreatedAt { get; }

    public TaskCreatedEvent(
        Guid taskId,
        int tenantId,
        Guid projectId,
        string title,
        TaskStatusContract status,
        TaskPriorityContract priority,
        Guid createdByUserId,
        string? description = null,
        Guid? assignedToUserId = null,
        DateTime? dueDate = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        TaskId = taskId;
        TenantId = tenantId;
        ProjectId = projectId;
        Title = title;
        Description = description;
        Status = status;
        Priority = priority;
        AssignedToUserId = assignedToUserId;
        CreatedByUserId = createdByUserId;
        DueDate = dueDate;
        CreatedAt = occurredAt ?? DateTime.UtcNow;
    }
}

