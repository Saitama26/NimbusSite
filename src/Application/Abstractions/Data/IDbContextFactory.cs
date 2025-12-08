namespace Application.Abstractions.Data;

/// <summary>
/// Фабрика для создания IApplicationDbContext с правильным connection string для шарда
/// </summary>
public interface IDbContextFactory
{
    /// <summary>
    /// Создать IApplicationDbContext для указанного TenantId
    /// </summary>
    Task<IApplicationDbContext> CreateDbContextAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать IApplicationDbContext для текущего tenant из контекста
    /// </summary>
    Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
}

