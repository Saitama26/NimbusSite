using Tenants.Contracts.Enums;

namespace Tenants.Contracts.Views;

/// <summary>
/// View модель тенанта для других модулей (read-only)
/// Соответствует Database View vw_Tenants
/// </summary>
public sealed class TenantView
{
    public int TenantInt { get; set; }
    public string Name { get; set; } = string.Empty;
    public TenantStatusContract Status { get; set; }
    public string? ConnectionString { get; set; }
    public DateTime CreatedAt { get; set; }
}

