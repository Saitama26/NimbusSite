namespace Tasks.Application.DTOs;

/// <summary>
/// Полная информация о задаче
/// </summary>
public sealed class TaskDto
{
    /// <summary>
    /// Идентификатор задачи
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Идентификатор проекта
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Название задачи
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи
    /// </summary>
    public string? Description { get; set; }

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
    /// Идентификатор пользователя, создавшего задачу
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Срок выполнения задачи
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Дата создания задачи
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата последнего обновления задачи
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Дата начала выполнения задачи
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Дата завершения задачи
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

