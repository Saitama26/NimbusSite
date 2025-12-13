namespace Tasks.Application.DTOs;

/// <summary>
/// Краткая информация о задаче для списков
/// </summary>
public sealed class TaskListItemDto
{
    /// <summary>
    /// Идентификатор задачи
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор проекта
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Название задачи
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Статус задачи
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Приоритет задачи
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор пользователя, которому назначена задача
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Срок выполнения задачи
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Дата создания задачи
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

