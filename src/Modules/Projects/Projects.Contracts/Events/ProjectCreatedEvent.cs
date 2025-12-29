using Common.Domain.Events;
using Projects.Contracts.Enums;

namespace Projects.Contracts.Events;

/// <summary>
/// Интеграционное событие создания проекта
/// Публикуется через Kafka для других модулей
/// </summary>
public sealed class ProjectCreatedEvent : BaseIntegrationEvent
{
    public Guid ProjectId { get; }
    public int TenantId { get; }
    public Guid CreatedByUserId { get; }
    public string Name { get; }
    public string? Description { get; }
    public ProjectStatusContract Status { get; }

    public ProjectCreatedEvent(
        Guid projectId,
        int tenantId,
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

