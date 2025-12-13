using Common.Domain.Events;

namespace Contracts.Identity.Events;

/// <summary>
/// Интеграционное событие входа пользователя в систему
/// </summary>
public sealed class UserLoggedInEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid SessionId { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public DateTime LoggedInAt { get; }

    public UserLoggedInEvent(
        Guid userId,
        Guid tenantId,
        Guid sessionId,
        DateTime? occurredAt = null,
        string? ipAddress = null,
        string? userAgent = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        SessionId = sessionId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LoggedInAt = occurredAt ?? DateTime.UtcNow;
    }
}

