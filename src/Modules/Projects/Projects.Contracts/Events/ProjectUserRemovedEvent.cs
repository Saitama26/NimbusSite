using Common.Domain.Events;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления пользователя из проекта
/// </summary>
public sealed class ProjectUserRemovedEvent : BaseIntegrationEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public Guid UserId { get; }

    public ProjectUserRemovedEvent(
        Guid projectUserId,
        Guid projectId,
        int tenantId,
        Guid userId,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectUserId = projectUserId;
        ProjectId = projectId;
        TenantId = tenantId;
        UserId = userId;
    }
}

