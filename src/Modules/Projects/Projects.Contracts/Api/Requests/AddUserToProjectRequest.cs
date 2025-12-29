using Users.Contracts.Enums;

namespace Projects.Contracts.Api.Requests;

/// <summary>
/// Запрос на добавление пользователя в проект
/// </summary>
public sealed record AddUserToProjectRequest(
    Guid UserId,
    UserRoleContract Role);

