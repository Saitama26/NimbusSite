namespace Users.Application.Abstractions;

/// <summary>
/// Интерфейс Unit of Work для управления транзакциями
/// Реализация будет в Infrastructure слое
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить все изменения в базе данных
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

