using Common.Domain.Events;
using Contracts.Users;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие добавления пользователя в проект
/// </summary>
public sealed class ProjectUserCreatedEvent : BaseDomainEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public UserRoleContract Role { get; }
    public DateTime JoinedAt { get; }

    public ProjectUserCreatedEvent(
        Guid projectUserId,
        Guid projectId,
        Guid tenantId,
        Guid userId,
        UserRoleContract role,
        DateTime joinedAt,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectUserId = projectUserId;
        ProjectId = projectId;
        TenantId = tenantId;
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
}

