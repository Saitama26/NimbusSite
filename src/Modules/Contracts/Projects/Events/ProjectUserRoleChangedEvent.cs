using Common.Domain.Events;
using Contracts.Users;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие изменения роли пользователя в проекте
/// </summary>
public sealed class ProjectUserRoleChangedEvent : BaseDomainEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public UserRoleContract OldRole { get; }
    public UserRoleContract NewRole { get; }

    public ProjectUserRoleChangedEvent(
        Guid projectUserId,
        Guid projectId,
        Guid tenantId,
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

