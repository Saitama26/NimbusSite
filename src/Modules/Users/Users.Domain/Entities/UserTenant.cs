using Common.Domain.Entities;
using Users.Domain.Enums;

namespace Users.Domain.Entities;

/// <summary>
/// Связь Many-to-Many между User и Tenant
/// Определяет, в каких тенантах находится пользователь и с какой ролью
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
    /// ОГРАНИЧЕНИЕ: IsOwner=true может быть только для ОДНОГО тенанта
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

    // Навигационные свойства
    public User User { get; set; } = null!;
    // Tenant будет в модуле Tenants, поэтому здесь только TenantId
}
