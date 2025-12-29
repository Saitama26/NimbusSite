namespace Common.Application.Abstractions.Tenancy;

/// <summary>
/// Менеджер для получения connection string тенанта по его числовому идентификатору
/// Использует Shard Map для маппинга tenantInt → connection string
/// Использует кэширование (TTL 5 минут) для оптимизации производительности
/// </summary>
public interface IShardMapManager
{
    /// <summary>
    /// Получить connection string для тенанта по его числовому идентификатору
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Connection string для подключения к базе данных тенанта</returns>
    Task<string> GetConnectionStringAsync(int tenantInt, CancellationToken cancellationToken = default);
}

