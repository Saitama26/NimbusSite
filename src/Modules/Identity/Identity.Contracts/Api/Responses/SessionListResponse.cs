using Identity.Contracts.Enums;

namespace Identity.Contracts.Api.Responses;

/// <summary>
/// Ответ со списком сессий
/// </summary>
public sealed record SessionListResponse(
    Guid Id,
    Guid UserId,
    int TenantId,
    SessionStatusContract Status,
    string? IpAddress,
    string? UserAgent,
    DateTime LastActivityAt,
    DateTime CreatedAt);

