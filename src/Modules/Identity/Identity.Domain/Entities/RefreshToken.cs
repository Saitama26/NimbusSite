using Common.Domain.Entities;

namespace Identity.Domain.Entities;

/// <summary>
/// Токен обновления для аутентификации пользователя
/// </summary>
public class RefreshToken : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Идентификатор пользователя (из модуля Users)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Значение токена (хеш)
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Дата истечения токена
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Дата отзыва токена (если токен был отозван)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP адрес, с которого был создан токен
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent браузера/клиента
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Причина отзыва токена
    /// </summary>
    public string? RevocationReason { get; set; }

    /// <summary>
    /// Проверяет, действителен ли токен (не истек и не отозван)
    /// </summary>
    public bool IsValid => ExpiresAt > DateTime.UtcNow && RevokedAt == null;

    // EF Core требует конструктор без параметров
    public RefreshToken() : base() { }
}

