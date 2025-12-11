using Users.Domain.Entities;

namespace Users.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для работы с пользователями
/// Реализация будет в Infrastructure слое
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по email в рамках тенанта
    /// </summary>
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить IQueryable всех пользователей для OData пагинации, фильтрации и сортировки
    /// </summary>
    Task<IQueryable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить IQueryable пользователей по тенанту
    /// </summary>
    Task<IQueryable<User>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование пользователя с указанным email в рамках тенанта
    /// </summary>
    Task<bool> ExistsByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить нового пользователя
    /// </summary>
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить пользователя (hard delete, если нужно)
    /// </summary>
    Task DeleteAsync(User user, CancellationToken cancellationToken = default);
}

