using Common.Domain.Entities;
using Identity.Domain.Enums;

namespace Identity.Domain.Entities;

/// <summary>
/// Сессия пользователя для отслеживания активных сеансов
/// Существует в tenant-специфичной БД со схемой Identity
/// </summary>
public class Session : BaseEntity
{
    /// <summary>
    /// Идентификатор пользователя (из модуля Users)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Tenant ID (числовой идентификатор тенанта)
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// Идентификатор токена обновления, связанного с сессией
    /// </summary>
    public Guid? RefreshTokenId { get; set; }

    /// <summary>
    /// Статус сессии
    /// </summary>
    public SessionStatus Status { get; set; }

    /// <summary>
    /// IP адрес, с которого была создана сессия
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent браузера/клиента
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Дата и время последней активности
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// Дата истечения сессии
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Дата и время закрытия сессии
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Причина закрытия сессии
    /// </summary>
    public string? CloseReason { get; set; }

    /// <summary>
    /// Проверяет, активна ли сессия
    /// </summary>
    public bool IsActive => Status == SessionStatus.Active && ExpiresAt > DateTime.UtcNow;

    // EF Core требует конструктор без параметров
    public Session() : base()
    {
        LastActivityAt = DateTime.UtcNow;
    }
}

