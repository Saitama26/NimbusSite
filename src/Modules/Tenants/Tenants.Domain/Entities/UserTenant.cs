using Common.Domain.Entities;
using Users.Domain.Enums;

namespace Tenants.Domain.Entities;

/// <summary>
/// Связь Many-to-Many между User и Tenant
/// Пользователь может быть в нескольких тенантах с разными ролями
/// Пользователь может быть владельцем (IsOwner=true) только одного тенанта
/// </summary>
public class UserTenant : BaseEntity
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Является ли пользователь владельцем тенанта
    /// ОГРАНИЧЕНИЕ: IsOwner=true может быть только для ОДНОГО тенанта у пользователя
    /// </summary>
    public bool IsOwner { get; set; }

    /// <summary>
    /// Роль пользователя в этом тенанте
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Дата присоединения к тенанту
    /// </summary>
    public DateTime JoinedAt { get; set; }

    // EF Core требует конструктор без параметров
    public UserTenant() : base() { }
}

