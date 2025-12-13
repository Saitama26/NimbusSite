using Common.Domain.Events;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие удаления пользователя из проекта
/// </summary>
public sealed class ProjectUserRemovedEvent : BaseDomainEvent
{
    public Guid ProjectUserId { get; }
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public Guid UserId { get; }

    public ProjectUserRemovedEvent(
        Guid projectUserId,
        Guid projectId,
        Guid tenantId,
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

