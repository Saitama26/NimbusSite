using System.Text.Json.Serialization;
using Common.Domain.Events;
using Users.Contracts.Enums;

namespace Users.Contracts.Events;

/// <summary>
/// Интеграционное событие изменения статуса пользователя
/// </summary>
public sealed class UserStatusChangedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; init; }
    public int TenantId { get; init; }
    public UserStatusContract OldStatus { get; init; }
    public UserStatusContract NewStatus { get; init; }

    // Параметрless конструктор для System.Text.Json десериализации
    [JsonConstructor]
    public UserStatusChangedEvent() : base()
    {
    }

    // Конструктор для создания события в коде
    public UserStatusChangedEvent(
        Guid userId,
        int tenantId,
        UserStatusContract oldStatus,
        UserStatusContract newStatus,
        DateTime? occurredOn = null,
        Guid? eventId = null)
        : base(eventId ?? Guid.NewGuid(), occurredOn ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

