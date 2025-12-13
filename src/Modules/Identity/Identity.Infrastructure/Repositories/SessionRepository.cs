using Microsoft.EntityFrameworkCore;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Infrastructure;

namespace Identity.Infrastructure.Repositories;

internal sealed class SessionRepository : ISessionRepository
{
    private readonly IdentityDbContext _dbContext;

    public SessionRepository(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken);
    }

    public Task<IQueryable<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.Sessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .AsQueryable());
    }

    public Task<IQueryable<Session>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_dbContext.Sessions
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .AsQueryable());
    }

    public async Task<Session?> GetByRefreshTokenIdAsync(Guid refreshTokenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefreshTokenId == refreshTokenId, cancellationToken);
    }

    public async Task AddAsync(Session session, CancellationToken cancellationToken = default)
    {
        await _dbContext.Sessions.AddAsync(session, cancellationToken);
    }

    public Task UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        _dbContext.Sessions.Update(session);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Session session, CancellationToken cancellationToken = default)
    {
        _dbContext.Sessions.Remove(session);
        return Task.CompletedTask;
    }
}

