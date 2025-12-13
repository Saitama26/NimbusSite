namespace Contracts.Users;

/// <summary>
/// Контракт для обмена данными о пользователе между модулями
/// </summary>
public sealed record UserContract(
    Guid Id,
    Guid TenantId,
    string Email,
    string Name,
    UserStatusContract Status,
    UserRoleContract Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null,
    DateTime? LastLoginAt = null,
    string? Phone = null,
    string? Bio = null);

/// <summary>
/// Статус пользователя
/// </summary>
public enum UserStatusContract
{
    Active = 1,
    Inactive = 2,
    Deleted = 3
}

/// <summary>
/// Роль пользователя
/// </summary>
public enum UserRoleContract
{
    Admin = 1,
    Manager = 2,
    User = 3,
    Guest = 4
}

