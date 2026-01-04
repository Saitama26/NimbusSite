using Users.Contracts.Enums;

namespace Users.Contracts.Api.Requests;

/// <summary>
/// Запрос на изменение статуса пользователя
/// </summary>
public sealed record ChangeUserStatusRequest(
    UserStatusContract Status);

