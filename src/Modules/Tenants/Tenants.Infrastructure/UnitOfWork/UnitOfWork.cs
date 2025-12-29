using Common.Application.Abstractions;
using Tenants.Infrastructure.Persistence;

namespace Tenants.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly TenantsDbContext _dbContext;

    public UnitOfWork(TenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}