using Common.Domain.Events;

namespace Contracts.Projects.Events;

/// <summary>
/// Интеграционное событие создания проекта.
/// </summary>
public sealed class ProjectCreatedEvent : BaseDomainEvent
{
    public Guid ProjectId { get; }
    public Guid TenantId { get; }
    public Guid CreatedByUserId { get; }
    public string Name { get; }
    public string? Description { get; }
    public ProjectStatusContract Status { get; }

    public ProjectCreatedEvent(
        Guid projectId,
        Guid tenantId,
        Guid createdByUserId,
        string name,
        ProjectStatusContract status,
        string? description = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        ProjectId = projectId;
        TenantId = tenantId;
        CreatedByUserId = createdByUserId;
        Name = name;
        Status = status;
        Description = description;
    }
}

