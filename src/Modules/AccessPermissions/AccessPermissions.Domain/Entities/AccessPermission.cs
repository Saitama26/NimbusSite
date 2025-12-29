using Common.Domain.Entities;
using AccessPermissions.Domain.Enums;

namespace AccessPermissions.Domain.Entities;

/// <summary>
/// Сущность разрешения доступа
/// Определяет права пользователя на выполнение действий в рамках тенанта или проекта
/// </summary>
public class AccessPermission : BaseEntity
{
    /// <summary>
    /// Идентификатор тенанта (числовой)
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Идентификатор пользователя, которому предоставлено разрешение
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор проекта (nullable, если разрешение на уровне тенанта)
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Идентификатор задачи (nullable, если разрешение на уровне задачи)
    /// </summary>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Область действия разрешения
    /// </summary>
    public PermissionScope Scope { get; set; }

    /// <summary>
    /// Действие, на которое распространяется разрешение
    /// </summary>
    public PermissionAction Action { get; set; }

    /// <summary>
    /// Тип разрешения (что разрешено делать)
    /// </summary>
    public PermissionType Type { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего разрешение
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Дата истечения разрешения (nullable, если бессрочное)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Примечание к разрешению
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Проверяет, действительно ли разрешение (не истекло)
    /// </summary>
    public bool IsValid => ExpiresAt == null || ExpiresAt > DateTime.UtcNow;

    // EF Core требует конструктор без параметров
    public AccessPermission() : base() { }
}

