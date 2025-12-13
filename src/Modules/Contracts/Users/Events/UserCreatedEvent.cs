using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие создания нового пользователя
/// Пользователь создается без тенанта, связь с тенантом создается через UserTenant
/// </summary>
public sealed class UserCreatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string Name { get; }
    public UserStatusContract Status { get; }
    public string? Phone { get; }
    public string? Bio { get; }

    public UserCreatedEvent(
        Guid userId,
        string email,
        string name,
        UserStatusContract status,
        string? phone = null,
        string? bio = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        Email = email;
        Name = name;
        Status = status;
        Phone = phone;
        Bio = bio;
    }
}

