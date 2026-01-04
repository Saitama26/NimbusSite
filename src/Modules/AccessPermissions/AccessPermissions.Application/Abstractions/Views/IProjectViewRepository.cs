using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AccessPermissions.Application.Abstractions.Views;

public interface IProjectViewRepository
{
    Task<ProjectViewDto?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectViewDto>> GetByIdsAsync(IEnumerable<Guid> projectIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
}

