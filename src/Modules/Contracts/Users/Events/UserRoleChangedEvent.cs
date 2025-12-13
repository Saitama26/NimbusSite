using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие изменения роли пользователя
/// </summary>
public sealed class UserRoleChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public UserRoleContract OldRole { get; }
    public UserRoleContract NewRole { get; }

    public UserRoleChangedEvent(
        Guid userId,
        Guid tenantId,
        UserRoleContract oldRole,
        UserRoleContract newRole,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}

