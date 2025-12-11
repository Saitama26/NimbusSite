using Users.Domain.Enums;

namespace Users.Application.DTOs;

/// <summary>
/// DTO для полной информации о пользователе
/// </summary>
public sealed record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string Name,
    UserStatus Status,
    UserRole Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    string? Phone,
    string? Bio);

