using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие обновления пользователя
/// </summary>
public sealed class UserUpdatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string? Name { get; }
    public string? Phone { get; }
    public string? Bio { get; }

    public UserUpdatedEvent(
        Guid userId,
        string? name = null,
        string? phone = null,
        string? bio = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        Name = name;
        Phone = phone;
        Bio = bio;
    }
}

