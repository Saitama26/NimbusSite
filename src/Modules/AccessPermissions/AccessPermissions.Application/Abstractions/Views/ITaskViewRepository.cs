using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AccessPermissions.Application.Abstractions.Views;

public interface ITaskViewRepository
{
    Task<TaskViewDto?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskViewDto>> GetByIdsAsync(IEnumerable<Guid> taskIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid taskId, CancellationToken cancellationToken = default);
}

