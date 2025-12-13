using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с токенами обновления
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Получить токен по ID
    /// </summary>
    Task<RefreshToken?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить токен по хешу
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все активные токены пользователя
    /// </summary>
    Task<IQueryable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все активные токены тенанта
    /// </summary>
    Task<IQueryable<RefreshToken>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новый токен
    /// </summary>
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить токен
    /// </summary>
    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить токен
    /// </summary>
    Task DeleteAsync(RefreshToken token, CancellationToken cancellationToken = default);
}

