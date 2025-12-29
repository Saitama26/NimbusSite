using Tasks.Contracts.Enums;

namespace Tasks.Contracts.Views;

/// <summary>
/// Database View модель задачи для других модулей (read-only)
/// Соответствует Database View vw_Tasks
/// </summary>
public sealed class TaskView
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusContract Status { get; set; }
    public TaskPriorityContract Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

