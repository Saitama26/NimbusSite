using System.Text.Json.Serialization;
using Common.Domain.Events;
using Users.Contracts.Enums;

namespace Users.Contracts.Events;

/// <summary>
/// Интеграционное событие создания нового пользователя
/// Пользователь создается в tenant-специфичной БД со схемой Users
/// </summary>
public sealed class UserCreatedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public int TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserStatusContract Status { get; init; }
    public string? Phone { get; init; }
    public string? Bio { get; init; }

    // Параметрless конструктор для System.Text.Json десериализации
    [JsonConstructor]
    public UserCreatedEvent() : base()
    {
    }

    // Конструктор для создания события в коде
    public UserCreatedEvent(
        Guid userId,
        int tenantId,
        string email,
        string name,
        UserStatusContract status,
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
        Phone = phone;
        Bio = bio;
    }
}

