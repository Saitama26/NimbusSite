using Common.Domain.Events;

namespace Contracts.AccessPermissions.Events;

/// <summary>
/// Интеграционное событие удаления разрешения доступа
/// </summary>
public sealed class AccessPermissionDeletedEvent : BaseDomainEvent
{
    public Guid PermissionId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }

    public AccessPermissionDeletedEvent(
        Guid permissionId,
        Guid tenantId,
        Guid userId,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        PermissionId = permissionId;
        TenantId = tenantId;
        UserId = userId;
    }
}

