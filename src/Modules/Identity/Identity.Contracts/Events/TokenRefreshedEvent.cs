using Common.Domain.Events;

namespace Identity.Contracts.Events;

/// <summary>
/// Интеграционное событие обновления токена
/// </summary>
public sealed class TokenRefreshedEvent : BaseIntegrationEvent
{
    public Guid UserId { get; }
    public int TenantId { get; }
    public Guid OldRefreshTokenId { get; }
    public Guid NewRefreshTokenId { get; }
    public DateTime RefreshTokenExpiresAt { get; }
    public DateTime? RefreshedAt { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }

    public TokenRefreshedEvent(
        Guid userId,
        int tenantId,
        Guid oldRefreshTokenId,
        Guid newRefreshTokenId,
        DateTime refreshTokenExpiresAt,
        DateTime? refreshedAt = null,
        string? ipAddress = null,
        string? userAgent = null,
        DateTime? occurredAt = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldRefreshTokenId = oldRefreshTokenId;
        NewRefreshTokenId = newRefreshTokenId;
        RefreshTokenExpiresAt = refreshTokenExpiresAt;
        RefreshedAt = refreshedAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}

