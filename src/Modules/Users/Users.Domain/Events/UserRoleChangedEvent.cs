using Common.Domain.Events;
using Users.Domain.Enums;

namespace Users.Domain.Events;

/// <summary>
/// Интеграционное событие изменения роли пользователя
/// </summary>
public sealed class UserRoleChangedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public UserRole OldRole { get; }
    public UserRole NewRole { get; }

    public UserRoleChangedEvent(
        Guid userId,
        Guid tenantId,
        UserRole oldRole,
        UserRole newRole,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}

