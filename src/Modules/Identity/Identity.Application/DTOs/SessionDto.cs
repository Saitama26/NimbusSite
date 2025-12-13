using Identity.Domain.Enums;

namespace Identity.Application.DTOs;

/// <summary>
/// DTO для информации о сессии
/// </summary>
public sealed class SessionDto
{
    /// <summary>
    /// Идентификатор сессии
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Статус сессии
    /// </summary>
    public SessionStatus Status { get; set; }

    /// <summary>
    /// IP адрес
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Дата последней активности
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    /// <summary>
    /// Дата создания сессии
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата истечения сессии
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

