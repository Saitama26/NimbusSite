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
    /// Получить пользователя по email (глобально уникальный)
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить IQueryable всех пользователей для OData пагинации, фильтрации и сортировки
    /// </summary>
    Task<IQueryable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование пользователя с указанным email (глобально)
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

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

