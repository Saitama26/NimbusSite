using Common.Domain.Events;
using Users.Domain.Enums;

namespace Users.Domain.Events;

/// <summary>
/// Интеграционное событие создания нового пользователя
/// </summary>
public sealed class UserCreatedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public string Email { get; }
    public string Name { get; }
    public UserRole Role { get; }
    public UserStatus Status { get; }
    public string? Phone { get; }
    public string? Bio { get; }

    public UserCreatedEvent(
        Guid userId,
        Guid tenantId,
        string email,
        string name,
        UserRole role,
        UserStatus status,
        string? phone = null,
        string? bio = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        Email = email;
        Name = name;
        Role = role;
        Status = status;
        Phone = phone;
        Bio = bio;
    }
}

