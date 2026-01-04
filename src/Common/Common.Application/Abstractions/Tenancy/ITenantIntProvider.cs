namespace Common.Application.Abstractions.Tenancy;

/// <summary>
/// Провайдер для получения числового идентификатора тенанта по его имени
/// Использует кэширование (TTL 5 минут) для оптимизации производительности
/// </summary>
public interface ITenantIntProvider
{
    /// <summary>
    /// Получить числовой идентификатор тенанта по его имени
    /// </summary>
    /// <param name="tenantName">Имя тенанта (например, "cityofvancouver")</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Числовой идентификатор тенанта (tenantInt)</returns>
    Task<int> GetTenantIntAsync(string tenantName, CancellationToken cancellationToken = default);
}

