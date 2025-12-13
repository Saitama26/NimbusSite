using Common.Domain.Events;
using Contracts.Users;

namespace Contracts.Tenants.Events;

/// <summary>
/// Интеграционное событие создания связи User-Tenant
/// </summary>
public sealed class UserTenantCreatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public bool IsOwner { get; }
    public UserRoleContract Role { get; }
    public DateTime JoinedAt { get; }

    public UserTenantCreatedEvent(
        Guid userId,
        Guid tenantId,
        bool isOwner,
        UserRoleContract role,
        DateTime joinedAt,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        IsOwner = isOwner;
        Role = role;
        JoinedAt = joinedAt;
    }
}

