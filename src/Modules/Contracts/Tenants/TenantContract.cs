namespace Contracts.Tenants;

/// <summary>
/// Контракт для обмена данными о тенанте между модулями
/// </summary>
public sealed record TenantContract(
    Guid Id,
    string Name,
    string Subdomain,
    TenantStatusContract Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? Description = null,
    string? AdminEmail = null);

/// <summary>
/// Статус тенанта
/// </summary>
public enum TenantStatusContract
{
    Active = 1,
    Suspended = 2,
    Deleted = 3
}

