using Common.Domain.Events;

namespace Contracts.AccessPermissions.Events;

/// <summary>
/// Интеграционное событие обновления разрешения доступа
/// </summary>
public sealed class AccessPermissionUpdatedEvent : BaseDomainEvent
{
    public Guid PermissionId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public PermissionTypeContract Type { get; }
    public DateTime? ExpiresAt { get; }
    public string? Note { get; }

    public AccessPermissionUpdatedEvent(
        Guid permissionId,
        Guid tenantId,
        Guid userId,
        PermissionTypeContract type,
        DateTime? expiresAt = null,
        string? note = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        PermissionId = permissionId;
        TenantId = tenantId;
        UserId = userId;
        Type = type;
        ExpiresAt = expiresAt;
        Note = note;
    }
}

