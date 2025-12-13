using Tasks.Application.Abstractions;

namespace Tasks.Infrastructure.UnitOfWork;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly TasksDbContext _dbContext;

    public UnitOfWork(TasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

