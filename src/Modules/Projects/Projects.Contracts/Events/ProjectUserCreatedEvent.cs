using Common.Domain.Events;
using Users.Contracts.Enums;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие добавления пользователя в проект
/// </summary>
public sealed class ProjectUserCreatedEvent : BaseIntegrationEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }
    public UserRoleContract Role { get; }
    public DateTime JoinedAt { get; }

    public ProjectUserCreatedEvent(
        Guid projectUserId,
        Guid projectId,
        int tenantId,
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

