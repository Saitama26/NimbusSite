using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с учетными данными пользователей
/// </summary>
public interface IUserCredentialsRepository
{
    /// <summary>
    /// Получить учетные данные по ID пользователя
    /// </summary>
    Task<UserCredentials?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить учетные данные по email в рамках тенанта
    /// </summary>
    Task<UserCredentials?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить учетные данные по email (глобально, без привязки к тенанту)
    /// </summary>
    Task<UserCredentials?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование учетных данных для пользователя
    /// </summary>
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новые учетные данные
    /// </summary>
    Task AddAsync(UserCredentials credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить учетные данные
    /// </summary>
    Task UpdateAsync(UserCredentials credentials, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить учетные данные
    /// </summary>
    Task DeleteAsync(UserCredentials credentials, CancellationToken cancellationToken = default);
}

