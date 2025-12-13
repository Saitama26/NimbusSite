using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие удаления пользователя (soft delete)
/// </summary>
public sealed class UserDeletedEvent : BaseDomainEvent
{
    public Guid UserId { get; }

    public UserDeletedEvent(Guid userId, DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
    }
}

