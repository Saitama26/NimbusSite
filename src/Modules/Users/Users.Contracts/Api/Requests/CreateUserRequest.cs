namespace Users.Contracts.Api.Requests;

/// <summary>
/// Запрос на создание пользователя
/// </summary>
public sealed record CreateUserRequest(
    string Email,
    string Name,
    string? Phone = null,
    string? Bio = null);

