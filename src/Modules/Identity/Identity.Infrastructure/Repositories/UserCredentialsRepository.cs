using Microsoft.EntityFrameworkCore;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Infrastructure;

namespace Identity.Infrastructure.Repositories;

internal sealed class UserCredentialsRepository : IUserCredentialsRepository
{
    private readonly IdentityDbContext _dbContext;

    public UserCredentialsRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserCredentials?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<UserCredentials?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var emailLower = email.ToLowerInvariant().Trim();
        return await _dbContext.UserCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == emailLower, cancellationToken);
    }

    public async Task<UserCredentials?> GetByEmailGlobalAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailLower = email.ToLowerInvariant().Trim();
        return await _dbContext.UserCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == emailLower, cancellationToken);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserCredentials
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserCredentials.AddAsync(credentials, cancellationToken);
    }

    public Task UpdateAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
    {
        _dbContext.UserCredentials.Update(credentials);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserCredentials credentials, CancellationToken cancellationToken = default)
    {
        _dbContext.UserCredentials.Remove(credentials);
        return Task.CompletedTask;
    }
}

