using System.Text.Json.Serialization;
using Common.Domain.Events;
using Users.Contracts.Enums;

namespace Users.Contracts.Events;

/// <summary>
/// Интеграционное событие регистрации пользователя (с паролем)
/// Используется Identity модулем для создания UserCredentials
/// </summary>
public sealed class UserRegisteredEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public int TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserStatusContract Status { get; init; }
    public string PasswordHash { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Bio { get; init; }

    // Параметрless конструктор для System.Text.Json десериализации
    [JsonConstructor]
    public UserRegisteredEvent() : base()
    {
    }

    // Конструктор для создания события в коде
    public UserRegisteredEvent(
        Guid userId,
        int tenantId,
        string email,
        string name,
        UserStatusContract status,
        string passwordHash,
        string? phone = null,
        string? bio = null,
        DateTime? occurredOn = null,
        Guid? eventId = null)
        : base(eventId ?? Guid.NewGuid(), occurredOn ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        Email = email;
        Name = name;
        Status = status;
        PasswordHash = passwordHash;
        Phone = phone;
        Bio = bio;
    }
}

