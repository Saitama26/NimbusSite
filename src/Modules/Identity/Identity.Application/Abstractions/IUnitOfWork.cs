namespace Identity.Application.Abstractions;

/// <summary>
/// Единица работы для управления транзакциями
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить изменения в БД
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

