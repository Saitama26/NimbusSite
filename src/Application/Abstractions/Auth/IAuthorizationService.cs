using Domain.Permissions;

namespace Application.Abstractions.Auth;

/// <summary>
/// Сервис для проверки прав доступа пользователя
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Проверить, имеет ли пользователь указанное разрешение
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, имеет ли пользователь доступ к проекту
    /// </summary>
    Task<bool> HasProjectAccessAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, является ли пользователь владельцем проекта
    /// </summary>
    Task<bool> IsProjectOwnerAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить, имеет ли пользователь доступ к задаче
    /// </summary>
    Task<bool> HasTaskAccessAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
}

