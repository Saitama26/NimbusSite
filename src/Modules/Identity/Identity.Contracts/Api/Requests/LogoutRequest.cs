namespace Identity.Contracts.Api.Requests;

/// <summary>
/// Запрос на выход из системы
/// </summary>
public sealed record LogoutRequest(
    string? Reason = null);

