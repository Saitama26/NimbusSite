using Common.Domain.Events;

namespace Users.Domain.Events;

/// <summary>
/// Интеграционное событие обновления пользователя
/// </summary>
public sealed class UserUpdatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public string? Name { get; }
    public string? Phone { get; }
    public string? Bio { get; }

    public UserUpdatedEvent(
        Guid userId,
        Guid tenantId,
        string? name = null,
        string? phone = null,
        string? bio = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        Phone = phone;
        Bio = bio;
    }
}

