namespace Users.Contracts.Api.Requests;

/// <summary>
/// Запрос на регистрацию пользователя (с паролем)
/// </summary>
public sealed record RegisterUserRequest(
    string Email,
    string Name,
    string Password,
    string? Phone = null,
    string? Bio = null);

