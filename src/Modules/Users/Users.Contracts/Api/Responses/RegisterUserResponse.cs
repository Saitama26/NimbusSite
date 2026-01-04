namespace Users.Contracts.Api.Responses;

/// <summary>
/// Ответ при регистрации пользователя
/// </summary>
public sealed record RegisterUserResponse(
    Guid UserId);

