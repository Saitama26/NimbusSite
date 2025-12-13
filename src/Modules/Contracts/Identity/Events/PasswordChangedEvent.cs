using Common.Domain.Events;

namespace Contracts.Identity.Events;

/// <summary>
/// Интеграционное событие изменения пароля пользователя
/// </summary>
public sealed class PasswordChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public bool RevokeAllSessions { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public DateTime ChangedAt { get; }

    public PasswordChangedEvent(
        Guid userId,
        Guid tenantId,
        bool revokeAllSessions,
        DateTime? occurredAt = null,
        string? ipAddress = null,
        string? userAgent = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        RevokeAllSessions = revokeAllSessions;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ChangedAt = occurredAt ?? DateTime.UtcNow;
    }
}

