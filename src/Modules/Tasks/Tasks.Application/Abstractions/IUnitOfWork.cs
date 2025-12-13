namespace Tasks.Application.Abstractions;

/// <summary>
/// Unit of Work для Tasks (реализация в Infrastructure)
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

