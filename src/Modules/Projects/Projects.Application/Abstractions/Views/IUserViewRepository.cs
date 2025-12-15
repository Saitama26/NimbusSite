namespace Projects.Application.Abstractions.Views;

/// <summary>
/// Контракт чтения пользователей через Database View (Users) из других доменов.
/// </summary>
public interface IUserViewRepository
{
    Task<UserViewDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserViewDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

