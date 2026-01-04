using Common.Domain.Events;
using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления разрешения доступа
/// </summary>
public sealed class AccessPermissionUpdatedEvent : BaseIntegrationEvent
{
    public Guid PermissionId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }
    public PermissionTypeContract Type { get; }
    public DateTime? ExpiresAt { get; }
    public string? Note { get; }

    public AccessPermissionUpdatedEvent(
        Guid permissionId,
        int tenantId,
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

