using System.Text.Json.Serialization;
using Common.Domain.Events;

namespace Users.Contracts.Events;

/// <summary>
/// Интеграционное событие удаления пользователя (soft delete)
/// </summary>
public sealed class UserDeletedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public int TenantId { get; init; }

    // Параметрless конструктор для System.Text.Json десериализации
    [JsonConstructor]
    public UserDeletedEvent() : base()
    {
    }

    // Конструктор для создания события в коде
    public UserDeletedEvent(
        Guid userId,
        int tenantId,
        DateTime? occurredOn = null,
        Guid? eventId = null)
        : base(eventId ?? Guid.NewGuid(), occurredOn ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
    }
}

