namespace Common.Application.Abstractions.Tenancy;

/// <summary>
/// Главный интерфейс для работы с тенантами
/// Предоставляет API для получения информации о тенантах
/// </summary>
public interface ITenancyDomain
{
    /// <summary>
    /// Найти числовой идентификатор тенанта по его имени
    /// </summary>
    /// <param name="tenantName">Имя тенанта (например, "cityofvancouver")</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Числовой идентификатор тенанта (tenantInt)</returns>
    Task<int> FindTenantIntAsync(string tenantName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить информацию о тенанте по числовому идентификатору
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Информация о тенанте</returns>
    Task<TenantInfo> GetTenantInfoAsync(int tenantInt, CancellationToken cancellationToken = default);
}

/// <summary>
/// Информация о тенанте
/// </summary>
public sealed record TenantInfo
{
    public int TenantInt { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

