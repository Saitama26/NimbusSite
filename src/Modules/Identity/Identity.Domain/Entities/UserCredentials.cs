using Common.Domain.Entities;

namespace Identity.Domain.Entities;

/// <summary>
/// Учетные данные пользователя для аутентификации
/// Создается автоматически при создании пользователя через событие UserCreatedEvent
/// </summary>
public class UserCredentials : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Идентификатор пользователя (из модуля Users)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Идентификатор тенанта (nullable, так как пользователь может быть без тенанта)
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Email пользователя (дублируется из Users для быстрого поиска)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля (bcrypt/argon2)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Email подтвержден
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Дата последнего изменения пароля
    /// </summary>
    public DateTime? PasswordChangedAt { get; set; }

    /// <summary>
    /// Количество неудачных попыток входа
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Дата блокировки аккаунта (если превышен лимит попыток)
    /// </summary>
    public DateTime? LockedOutUntil { get; set; }

    /// <summary>
    /// Дата и время последнего обновления
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Проверяет, заблокирован ли аккаунт
    /// </summary>
    public bool IsLockedOut => LockedOutUntil.HasValue && LockedOutUntil.Value > DateTime.UtcNow;

    // EF Core требует конструктор без параметров
    public UserCredentials() : base() { }
}

