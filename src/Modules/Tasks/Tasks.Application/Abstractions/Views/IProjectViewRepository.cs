namespace Tasks.Application.Abstractions.Views;

/// <summary>
/// Контракт чтения проектов через Database View (Projects).
/// </summary>
public interface IProjectViewRepository
{
    Task<ProjectViewDto?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectViewDto>> GetByIdsAsync(IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
}

