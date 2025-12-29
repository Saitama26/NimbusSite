using Identity.Contracts.Enums;

namespace Identity.Contracts.Api.Responses;

/// <summary>
/// Ответ с информацией о сессии
/// </summary>
public sealed record SessionResponse(
    Guid Id,
    Guid UserId,
    int TenantId,
    SessionStatusContract Status,
    string? IpAddress,
    string? UserAgent,
    DateTime LastActivityAt,
    DateTime CreatedAt,
    DateTime ExpiresAt);

