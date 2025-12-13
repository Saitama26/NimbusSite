using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие изменения статуса пользователя
/// </summary>
public sealed class UserStatusChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public UserStatusContract OldStatus { get; }
    public UserStatusContract NewStatus { get; }

    public UserStatusChangedEvent(
        Guid userId,
        UserStatusContract oldStatus,
        UserStatusContract newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

