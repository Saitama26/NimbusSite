using Common.Domain.Events;

namespace Contracts.Identity.Events;

/// <summary>
/// Интеграционное событие выхода пользователя из системы
/// </summary>
public sealed class UserLoggedOutEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid SessionId { get; }
    public string? Reason { get; }
    public DateTime LoggedOutAt { get; }

    public UserLoggedOutEvent(
        Guid userId,
        Guid tenantId,
        Guid sessionId,
        DateTime? occurredAt = null,
        string? reason = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        SessionId = sessionId;
        Reason = reason;
        LoggedOutAt = occurredAt ?? DateTime.UtcNow;
    }
}

