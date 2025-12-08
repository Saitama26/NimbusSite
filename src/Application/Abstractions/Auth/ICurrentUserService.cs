namespace Application.Abstractions.Auth;

/// <summary>
/// Сервис для получения информации о текущем аутентифицированном пользователе
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID текущего пользователя
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email текущего пользователя
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// TenantId текущего пользователя
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Роль текущего пользователя
    /// </summary>
    Domain.Users.ValueObjects.UserRole? Role { get; }

    /// <summary>
    /// Проверить, аутентифицирован ли пользователь
    /// </summary>
    bool IsAuthenticated { get; }
}

