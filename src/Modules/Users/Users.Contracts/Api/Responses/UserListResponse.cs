using Users.Contracts.Enums;

namespace Users.Contracts.Api.Responses;

/// <summary>
/// Ответ со списком пользователей
/// </summary>
public sealed record UserListResponse(
    Guid Id,
    string Email,
    string Name,
    UserStatusContract Status,
    DateTime CreatedAt);

