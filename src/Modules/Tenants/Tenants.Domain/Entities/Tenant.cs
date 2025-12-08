using Common.Domain.Entities;
using Tenants.Domain.Enums;

namespace Tenants.Domain.Entities;

/// <summary>
/// Сущность тенанта - агрегатный корень домена Tenants
/// </summary>
public class Tenant : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Название тенанта
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Поддомен тенанта (уникальный идентификатор для URL)
    /// </summary>
    public string Subdomain { get; set; } = string.Empty;

    /// <summary>
    /// Строка подключения к базе данных тенанта (для шардирования)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Статус тенанта
    /// </summary>
    public TenantStatus Status { get; set; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Описание тенанта (опционально)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Email администратора тенанта
    /// </summary>
    public string? AdminEmail { get; set; }

    // EF Core требует конструктор без параметров
    public Tenant() : base() { }
}

