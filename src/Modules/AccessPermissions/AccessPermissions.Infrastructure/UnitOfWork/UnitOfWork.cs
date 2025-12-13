using AccessPermissions.Application.Abstractions;
using AccessPermissions.Infrastructure;

namespace AccessPermissions.Infrastructure.UnitOfWork;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AccessPermissionsDbContext _dbContext;

    public UnitOfWork(AccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

