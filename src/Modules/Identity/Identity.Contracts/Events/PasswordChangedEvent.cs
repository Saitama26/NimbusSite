using Common.Domain.Events;

namespace Identity.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения пароля пользователя
/// </summary>
public sealed class PasswordChangedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; }
    public int TenantId { get; }
    public bool RevokeAllSessions { get; }
    public DateTime ChangedAt { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }

    public PasswordChangedEvent(
        Guid userId,
        int tenantId,
        bool revokeAllSessions,
        DateTime changedAt,
        string? ipAddress = null,
        string? userAgent = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        RevokeAllSessions = revokeAllSessions;
        ChangedAt = changedAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}

