using Microsoft.EntityFrameworkCore;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Infrastructure;

namespace Identity.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentityDbContext _dbContext;

    public RefreshTokenRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == tokenId, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public Task<IQueryable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AsQueryable());
    }

    public Task<IQueryable<RefreshToken>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.RefreshTokens
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable());
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);
    }

    public Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(RefreshToken token, CancellationToken cancellationToken = default)
    {
        _dbContext.RefreshTokens.Remove(token);
        return Task.CompletedTask;
    }
}

