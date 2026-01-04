using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions;

/// <summary>
/// Интерфейс для доступа к DbContext без зависимости от Infrastructure
/// </summary>
public interface IIdentityDbContext
{
    DbSet<UserCredentials> UserCredentials { get; }
    DbSet<Session> Sessions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

