using System.Text.Json.Serialization;
using Common.Domain.Events;

namespace Users.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления пользователя
/// </summary>
public sealed class UserUpdatedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public int TenantId { get; init; }
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string? Bio { get; init; }

    // Параметрless конструктор для System.Text.Json десериализации
    [JsonConstructor]
    public UserUpdatedEvent() : base()
    {
    }

    // Конструктор для создания события в коде
    public UserUpdatedEvent(
        Guid userId,
        int tenantId,
        string? name = null,
        string? phone = null,
        string? bio = null,
        DateTime? occurredOn = null,
        Guid? eventId = null)
        : base(eventId ?? Guid.NewGuid(), occurredOn ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        Phone = phone;
        Bio = bio;
    }
}

