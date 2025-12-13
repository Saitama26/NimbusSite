using Common.Domain.Events;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие обновления проекта.
/// </summary>
public sealed class ProjectUpdatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public string? Name { get; }
    public string? Description { get; }

    public ProjectUpdatedEvent(
        Guid projectId,
        Guid tenantId,
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

