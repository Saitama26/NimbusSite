using Common.Domain.Events;

namespace Contracts.Users.Events;

/// <summary>
/// Интеграционное событие регистрации нового пользователя
/// Содержит хеш пароля для создания UserCredentials в Identity модуле
/// </summary>
public sealed class UserRegisteredEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string Name { get; }
    public UserStatusContract Status { get; }
    public string PasswordHash { get; } // Хеш пароля (не сам пароль!)
    public string? Phone { get; }
    public string? Bio { get; }

    public UserRegisteredEvent(
        Guid userId,
        string email,
        string name,
        UserStatusContract status,
        string passwordHash,
        string? phone = null,
        string? bio = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        Email = email;
        Name = name;
        Status = status;
        PasswordHash = passwordHash;
        Phone = phone;
        Bio = bio;
    }
}

