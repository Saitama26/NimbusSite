namespace Projects.Application.Abstractions;

/// <summary>
/// Unit of Work для Projects (реализация в Infrastructure).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

