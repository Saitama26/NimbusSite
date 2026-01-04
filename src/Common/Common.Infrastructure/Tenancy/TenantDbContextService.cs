using Common.Application.Abstractions.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Сервис для создания DbContext с динамическим connection string на основе TenantId
/// Может использоваться как в HTTP контексте (через HttpContext), так и в event handlers (с явным TenantId)
/// </summary>
public class TenantDbContextService
{
    private readonly IShardMapManager _shardMapManager;
    private readonly ILogger<TenantDbContextService> _logger;

    public TenantDbContextService(
        IShardMapManager shardMapManager,
        ILogger<TenantDbContextService> logger)
    {
        _shardMapManager = shardMapManager;
        _logger = logger;
    }

    /// <summary>
    /// Получить connection string для тенанта
    /// </summary>
    public async Task<string> GetConnectionStringAsync(int tenantInt, CancellationToken cancellationToken = default)
    {
        if (tenantInt <= 0)
        {
            throw new ArgumentException("TenantInt must be greater than zero", nameof(tenantInt));
        }

        return await _shardMapManager.GetConnectionStringAsync(tenantInt, cancellationToken);
    }
}

