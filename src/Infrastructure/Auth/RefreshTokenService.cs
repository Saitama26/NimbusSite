using Application.Abstractions.Auth;
using Application.Abstractions.Data;
using Domain.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth;

internal sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        IApplicationDbContext context,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Saving refresh token for user {UserId}, expires at {ExpiresAt}",
            userId,
            expiresAt);

        var tokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refresh token {TokenId} saved successfully for user {UserId}",
            tokenEntity.Id,
            userId);
    }

    public async Task<Guid?> GetUserIdByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.Token == refreshToken 
                    && !rt.IsRevoked 
                    && rt.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

        if (tokenEntity is null)
        {
            _logger.LogWarning("Refresh token not found or expired");
            return null;
        }

        return tokenEntity.UserId;
    }

    public async Task InvalidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating refresh token");

        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity is null)
        {
            _logger.LogWarning("Refresh token not found for invalidation");
            return;
        }

        tokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refresh token {TokenId} invalidated successfully",
            tokenEntity.Id);
    }

    public async Task InvalidateAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Invalidating all refresh tokens for user {UserId}", userId);

        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        if (tokens.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Invalidated {Count} refresh token(s) for user {UserId}",
                tokens.Count,
                userId);
        }
        else
        {
            _logger.LogInformation("No active refresh tokens found for user {UserId}", userId);
        }
    }

    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (tokenEntity is null)
        {
            _logger.LogDebug("Refresh token not found");
            return false;
        }

        if (tokenEntity.IsRevoked)
        {
            _logger.LogDebug("Refresh token {TokenId} is revoked", tokenEntity.Id);
            return false;
        }

        if (tokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogDebug("Refresh token {TokenId} has expired", tokenEntity.Id);
            return false;
        }

        return true;
    }
}

