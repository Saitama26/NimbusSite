namespace Application.Abstractions.Data;

/// <summary>
/// Контекст для получения информации о текущем тенанте (для шардирования)
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// ID текущего тенанта
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Установить текущий тенант
    /// </summary>
    void SetTenantId(Guid tenantId);

    /// <summary>
    /// Очистить текущий тенант
    /// </summary>
    void ClearTenantId();
}

