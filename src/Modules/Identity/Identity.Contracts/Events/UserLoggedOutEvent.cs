using Common.Domain.Events;

namespace Identity.Contracts.Events;

/// <summary>
/// Интеграционное событие выхода пользователя из системы
/// </summary>
public sealed class UserLoggedOutEvent : BaseIntegrationEvent
{
    public Guid UserId { get; }
    public int TenantId { get; }
    public Guid SessionId { get; }
    public DateTime? LoggedOutAt { get; }
    public string? Reason { get; }

    public UserLoggedOutEvent(
        Guid userId,
        int tenantId,
        Guid sessionId,
        DateTime? loggedOutAt = null,
        string? reason = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        SessionId = sessionId;
        LoggedOutAt = loggedOutAt;
        Reason = reason;
    }
}

