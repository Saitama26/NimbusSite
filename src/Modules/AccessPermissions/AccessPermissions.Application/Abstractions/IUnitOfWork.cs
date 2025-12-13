namespace AccessPermissions.Application.Abstractions;

/// <summary>
/// Unit of Work для управления транзакциями
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Сохранить изменения в базе данных
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

