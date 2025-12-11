using Common.Domain.Events;
using Users.Domain.Enums;

namespace Users.Domain.Events;

/// <summary>
/// Интеграционное событие изменения статуса пользователя
/// </summary>
public sealed class UserStatusChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public UserStatus OldStatus { get; }
    public UserStatus NewStatus { get; }

    public UserStatusChangedEvent(
        Guid userId,
        Guid tenantId,
        UserStatus oldStatus,
        UserStatus newStatus,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

