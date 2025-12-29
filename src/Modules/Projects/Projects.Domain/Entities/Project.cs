using Common.Domain.Entities;
using Projects.Domain.Enums;

namespace Projects.Domain.Entities;

/// <summary>
/// Сущность проекта
/// Проект существует в tenant-специфичной БД со схемой Projects
/// </summary>
public class Project : BaseEntity
{
    /// <summary>
    /// Tenant ID (числовой идентификатор тенанта)
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Название проекта.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание проекта.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Статус проекта.
    /// </summary>
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// Дата и время последнего обновления.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    // EF Core требует конструктор без параметров
    public Project() : base() { }
}

