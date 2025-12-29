using Common.Domain.Events;
using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Events;

/// <summary>
/// Интеграционное событие создания разрешения доступа
/// </summary>
public sealed class AccessPermissionCreatedEvent : BaseIntegrationEvent
{
    public Guid PermissionId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }
    public Guid? ProjectId { get; }
    public Guid? TaskId { get; }
    public PermissionScopeContract Scope { get; }
    public PermissionActionContract Action { get; }
    public PermissionTypeContract Type { get; }
    public Guid CreatedByUserId { get; }
    public DateTime? ExpiresAt { get; }
    public string? Note { get; }

    public AccessPermissionCreatedEvent(
        Guid permissionId,
        int tenantId,
        Guid userId,
        PermissionScopeContract scope,
        PermissionActionContract action,
        PermissionTypeContract type,
        Guid createdByUserId,
        Guid? projectId = null,
        Guid? taskId = null,
        DateTime? expiresAt = null,
        string? note = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        PermissionId = permissionId;
        TenantId = tenantId;
        UserId = userId;
        ProjectId = projectId;
        TaskId = taskId;
        Scope = scope;
        Action = action;
        Type = type;
        CreatedByUserId = createdByUserId;
        ExpiresAt = expiresAt;
        Note = note;
    }
}

