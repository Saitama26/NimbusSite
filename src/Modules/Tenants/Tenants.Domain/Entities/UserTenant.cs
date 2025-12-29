using Common.Domain.Entities;
using Users.Domain.Enums;

namespace Tenants.Domain.Entities;

/// <summary>
/// Связь Many-to-Many между User и Tenant
/// Пользователь может быть в нескольких тенантах с разными ролями
/// Пользователь может быть владельцем (IsOwner=true) только одного тенанта
/// Хранится в центральной БД NimbusSite_Tenants (таблицы в корне базы данных без схем)
/// </summary>
public class UserTenant : IBaseEntity
{
    /// <summary>
    /// Идентификатор связи (Primary Key)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Числовой идентификатор тенанта
    /// </summary>
    public int TenantInt { get; set; }

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

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public UserTenant()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        JoinedAt = DateTime.UtcNow;
    }
}

