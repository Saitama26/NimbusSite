using Common.Domain.Events;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления проекта
/// </summary>
public sealed class ProjectUpdatedEvent : BaseIntegrationEvent
{
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public string? Name { get; }
    public string? Description { get; }

    public ProjectUpdatedEvent(
        Guid projectId,
        int tenantId,
        string? name = null,
        string? description = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
        Name = name;
        Description = description;
    }
}

