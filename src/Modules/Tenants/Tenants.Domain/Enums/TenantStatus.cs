namespace Tenants.Domain.Enums;

/// <summary>
/// Статус тенанта
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Активен
    /// </summary>
    Active = 1,

    /// <summary>
    /// Приостановлен
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// Удален
    /// </summary>
    Deleted = 3
}

