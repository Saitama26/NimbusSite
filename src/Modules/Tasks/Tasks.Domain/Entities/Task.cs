using Common.Domain.Entities;
using Tasks.Domain.Enums;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;

namespace Tasks.Domain.Entities;

/// <summary>
/// Сущность задачи
/// Задача существует в tenant-специфичной БД со схемой Tasks
/// </summary>
public class Task : BaseEntity
{
    /// <summary>
    /// Tenant ID (числовой идентификатор тенанта)
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Идентификатор проекта, к которому относится задача
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
    public TaskStatus Status { get; set; }

    /// <summary>
    /// Приоритет задачи
    /// </summary>
    public TaskPriority Priority { get; set; }

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
    /// Дата и время последнего обновления
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

    // EF Core требует конструктор без параметров
    public Task() : base() { }
}

