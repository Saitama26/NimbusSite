using Common.Application.Abstractions;

namespace Identity.Infrastructure.UnitOfWork;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _dbContext;

    public UnitOfWork(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}

