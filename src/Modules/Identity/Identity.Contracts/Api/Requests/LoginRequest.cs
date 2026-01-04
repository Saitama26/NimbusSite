namespace Identity.Contracts.Api.Requests;

/// <summary>
/// Запрос на вход пользователя
/// TenantId передается через query параметр, а не в теле запроса
/// </summary>
public sealed record LoginRequest(
    string Email,
    string Password);

