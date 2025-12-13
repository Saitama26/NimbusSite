using Common.Domain.Events;

namespace Contracts.Identity.Events;

/// <summary>
/// Интеграционное событие генерации токена
/// </summary>
public sealed class TokenGeneratedEvent : BaseDomainEvent
{
    public Guid UserId { get; }
    public Guid TenantId { get; }
    public Guid? RefreshTokenId { get; }
    public TokenTypeContract TokenType { get; }
    public DateTime ExpiresAt { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public DateTime GeneratedAt { get; }

    public TokenGeneratedEvent(
        Guid userId,
        Guid tenantId,
        TokenTypeContract tokenType,
        DateTime expiresAt,
        DateTime? occurredAt = null,
        Guid? refreshTokenId = null,
        string? ipAddress = null,
        string? userAgent = null)
        : base(Guid.NewGuid(), occurredAt ?? DateTime.UtcNow)
    {
        UserId = userId;
        TenantId = tenantId;
        RefreshTokenId = refreshTokenId;
        TokenType = tokenType;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        GeneratedAt = occurredAt ?? DateTime.UtcNow;
    }
}

