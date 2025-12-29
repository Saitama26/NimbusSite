namespace Users.Contracts.Api.Responses;

/// <summary>
/// Ответ при создании пользователя
/// </summary>
public sealed record CreateUserResponse(
    Guid UserId);

