using Users.Domain.Enums;

namespace Users.Application.DTOs;

/// <summary>
/// DTO для краткой информации о пользователе в списке
/// </summary>
public sealed record UserListItemDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string Name,
    UserStatus Status,
    UserRole Role,
    DateTime CreatedAt);

