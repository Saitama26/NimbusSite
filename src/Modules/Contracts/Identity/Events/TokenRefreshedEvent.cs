using Common.Domain.Events;

namespace Contracts.Identity.Events;

/// <summary>
/// Интеграционное событие обновления токена
/// </summary>
public sealed class TokenRefreshedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid OldRefreshTokenId { get; }
    public Guid NewRefreshTokenId { get; }
    public DateTime ExpiresAt { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public DateTime RefreshedAt { get; }

    public TokenRefreshedEvent(
        Guid userId,
        Guid tenantId,
        Guid oldRefreshTokenId,
        Guid newRefreshTokenId,
        DateTime expiresAt,
        DateTime? occurredAt = null,
        string? ipAddress = null,
        string? userAgent = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        OldRefreshTokenId = oldRefreshTokenId;
        NewRefreshTokenId = newRefreshTokenId;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        RefreshedAt = occurredAt ?? DateTime.UtcNow;
    }
}

