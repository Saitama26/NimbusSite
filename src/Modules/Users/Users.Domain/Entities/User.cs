using Common.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Domain.Entities;

/// <summary>
/// Сущность пользователя - агрегатный корень домена Users
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Идентификатор тенанта, которому принадлежит пользователь
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Email пользователя (уникальный в рамках тенанта)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Статус пользователя
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Дата последнего входа (опционально)
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Телефон пользователя (опционально)
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Дополнительная информация о пользователе (опционально)
    /// </summary>
    public string? Bio { get; set; }

    // EF Core требует конструктор без параметров
    public User() : base() { }
}

