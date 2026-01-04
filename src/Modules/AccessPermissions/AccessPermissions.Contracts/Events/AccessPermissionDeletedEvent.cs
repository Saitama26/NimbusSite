using Common.Domain.Events;

namespace AccessPermissions.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления разрешения доступа
/// </summary>
public sealed class AccessPermissionDeletedEvent : BaseIntegrationEvent
{
    public Guid PermissionId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }

    public AccessPermissionDeletedEvent(
        Guid permissionId,
        int tenantId,
        Guid userId,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        PermissionId = permissionId;
        TenantId = tenantId;
        UserId = userId;
    }
}

