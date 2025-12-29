using Common.Domain.Events;
using Users.Contracts.Enums;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения роли пользователя в проекте
/// </summary>
public sealed class ProjectUserRoleChangedEvent : BaseIntegrationEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }
    public UserRoleContract OldRole { get; }
    public UserRoleContract NewRole { get; }

    public ProjectUserRoleChangedEvent(
        Guid projectUserId,
        Guid projectId,
        int tenantId,
        Guid userId,
        UserRoleContract oldRole,
        UserRoleContract newRole,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectUserId = projectUserId;
        ProjectId = projectId;
        TenantId = tenantId;
        UserId = userId;
        OldRole = oldRole;
        NewRole = newRole;
    }
}

