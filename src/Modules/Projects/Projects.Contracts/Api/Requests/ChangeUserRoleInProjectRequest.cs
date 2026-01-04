using Users.Contracts.Enums;

namespace Projects.Contracts.Api.Requests;

/// <summary>
/// Запрос на изменение роли пользователя в проекте
/// </summary>
public sealed record ChangeUserRoleInProjectRequest(
    UserRoleContract Role);

