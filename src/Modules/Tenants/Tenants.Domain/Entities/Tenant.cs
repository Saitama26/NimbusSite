using Common.Domain.Entities;
using Tenants.Domain.Enums;

namespace Tenants.Domain.Entities;

/// <summary>
/// Сущность тенанта
/// Хранится в центральной БД NimbusSite_Tenants (таблицы в корне базы данных без схем)
/// </summary>
public class Tenant : IBaseEntity
{
    /// <summary>
    /// Числовой идентификатор тенанта (Primary Key, Auto Increment)
    /// </summary>
    public int TenantInt { get; set; }

    /// <summary>
    /// Название тенанта (уникальное)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Строка подключения к базе данных тенанта (для шардирования)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Статус тенанта
    /// </summary>
    public TenantStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Описание тенанта (опционально)
    /// </summary>
    public string? Description { get; set; }

    // Для совместимости с IBaseEntity (используем TenantInt как Id)
    Guid IBaseEntity.Id => Guid.Empty; // Не используется, используем TenantInt

    public Tenant()
    {
        CreatedAt = DateTime.UtcNow;
    }
}

