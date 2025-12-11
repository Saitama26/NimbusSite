using Common.Domain.Events;

namespace Users.Domain.Events;

/// <summary>
/// Интеграционное событие удаления пользователя (soft delete)
/// </summary>
public sealed class UserDeletedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }

    public UserDeletedEvent(Guid userId, Guid tenantId, DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
    }
}

