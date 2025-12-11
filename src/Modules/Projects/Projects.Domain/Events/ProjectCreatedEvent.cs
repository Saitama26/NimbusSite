using Common.Domain.Events;
using Projects.Domain.Enums;

namespace Projects.Domain.Events;

/// <summary>
/// Интеграционное событие создания проекта.
/// </summary>
public sealed class ProjectCreatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public string Name { get; }
    public string? Description { get; }
    public ProjectStatus Status { get; }

    public ProjectCreatedEvent(
        Guid projectId,
        Guid tenantId,
        string name,
        ProjectStatus status,
        string? description = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
        Name = name;
        Status = status;
        Description = description;
    }
}

