namespace Identity.Contracts.Api.Requests;

/// <summary>
/// Запрос на изменение пароля
/// </summary>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    bool RevokeAllSessions = true);

