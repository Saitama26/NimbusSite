using Common.Application.Abstractions;

namespace Projects.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ProjectsDbContext _dbContext;

    public UnitOfWork(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

