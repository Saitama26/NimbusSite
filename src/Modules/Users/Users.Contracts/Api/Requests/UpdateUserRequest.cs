namespace Users.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление пользователя
/// </summary>
public sealed record UpdateUserRequest(
    string? Name = null,
    string? Phone = null,
    string? Bio = null);

