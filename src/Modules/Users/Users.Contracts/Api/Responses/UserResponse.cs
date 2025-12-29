using Users.Contracts.Enums;

namespace Users.Contracts.Api.Responses;

/// <summary>
/// Ответ с информацией о пользователе
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string Name,
    UserStatusContract Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    string? Phone,
    string? Bio);

