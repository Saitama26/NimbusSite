using Common.Domain.Entities;
using Users.Domain.Enums;

namespace Projects.Domain.Entities;

/// <summary>
/// Связь Many-to-Many между Project и User
/// Пользователь может быть в нескольких проектах с разными ролями
/// </summary>
public class ProjectUser : BaseEntity
{
    /// <summary>
    /// Идентификатор проекта
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Роль пользователя в этом проекте
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Дата присоединения к проекту
    /// </summary>
    public DateTime JoinedAt { get; set; }

    // EF Core требует конструктор без параметров
    public ProjectUser() : base() { }
}

