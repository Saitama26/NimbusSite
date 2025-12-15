using System;
using System.Threading;
using System.Threading.Tasks;

namespace AccessPermissions.Application.Abstractions.Views;

public interface ITenantViewRepository
{
    Task<TenantViewDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

