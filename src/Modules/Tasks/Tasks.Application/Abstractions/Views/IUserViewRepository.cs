using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks.Application.Abstractions.Views;

/// <summary>
/// Контракт чтения пользователей через Database View (Users).
/// </summary>
public interface IUserViewRepository
{
    Task<UserViewDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserViewDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

