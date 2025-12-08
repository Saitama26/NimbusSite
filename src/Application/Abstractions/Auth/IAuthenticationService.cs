using Domain.Users;
using SharedKernel;

namespace Application.Abstractions.Auth;

/// <summary>
/// Сервис для аутентификации пользователей
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Аутентифицировать пользователя по email и паролю
    /// </summary>
    Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить access token используя refresh token
    /// </summary>
    Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выйти из системы (инвалидировать refresh token)
    /// </summary>
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Результат аутентификации
/// </summary>
public record AuthenticationResult(
    User User,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);

