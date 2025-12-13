using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

/// <summary>
/// Репозиторий для работы с сессиями пользователей
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Получить сессию по ID
    /// </summary>
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все активные сессии пользователя
    /// </summary>
    Task<IQueryable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить все активные сессии тенанта
    /// </summary>
    Task<IQueryable<Session>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить сессию по RefreshTokenId
    /// </summary>
    Task<Session?> GetByRefreshTokenIdAsync(Guid refreshTokenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить новую сессию
    /// </summary>
    Task AddAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить сессию
    /// </summary>
    Task UpdateAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить сессию
    /// </summary>
    Task DeleteAsync(Session session, CancellationToken cancellationToken = default);
}

