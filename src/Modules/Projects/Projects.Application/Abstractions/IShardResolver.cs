namespace Projects.Application.Abstractions;

/// <summary>
/// Абстракция для получения connection string по TenantId (шардирование).
/// </summary>
public interface IShardResolver
{
    Task<string?> ResolveConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

