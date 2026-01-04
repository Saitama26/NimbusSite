using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks.Application.Abstractions.Views;

/// <summary>
/// Контракт чтения тенантов через Database View (Tenants).
/// </summary>
public interface ITenantViewRepository
{
    Task<TenantViewDto?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

