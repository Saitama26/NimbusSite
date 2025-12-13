using Users.Domain.Enums;

namespace Users.Application.DTOs;

/// <summary>
/// DTO для полной информации о пользователе
/// </summary>
public sealed record UserDto(
    Guid Id,
    string Email,
    string Name,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    string? Phone,
    string? Bio);

