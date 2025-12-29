using Common.Domain.Results;

namespace Tenants.Domain.Errors;

/// <summary>
/// Ошибки домена Tenants
/// </summary>
public static class TenantErrors
{
    /// <summary>
    /// Тенант не найден
    /// </summary>
    public static Error NotFound(int tenantInt) =>
        Error.NotFound(
            "Tenant.NotFound",
            $"Tenant with TenantInt {tenantInt} was not found.");

    /// <summary>
    /// Тенант не найден по имени
    /// </summary>
    public static Error NotFoundByName(string name) =>
        Error.NotFound(
            "Tenant.NotFound",
            $"Tenant with name '{name}' was not found.");

    /// <summary>
    /// Тенант с указанным поддоменом уже существует
    /// </summary>
    public static Error SubdomainAlreadyExists(string subdomain) =>
        Error.Conflict(
            "Tenant.SubdomainAlreadyExists",
            $"Tenant with subdomain '{subdomain}' already exists.");

    /// <summary>
    /// Название тенанта не может быть пустым
    /// </summary>
    public static Error NameEmpty =>
        Error.Validation(
            "Tenant.NameEmpty",
            "Tenant name cannot be empty.");

    /// <summary>
    /// Поддомен не может быть пустым
    /// </summary>
    public static Error SubdomainEmpty =>
        Error.Validation(
            "Tenant.SubdomainEmpty",
            "Tenant subdomain cannot be empty.");

    /// <summary>
    /// Невалидный формат поддомена
    /// </summary>
    public static Error InvalidSubdomainFormat =>
        Error.Validation(
            "Tenant.InvalidSubdomainFormat",
            "Subdomain must contain only letters, numbers and hyphens, and be between 3 and 63 characters long.");

    /// <summary>
    /// Поддомен не может начинаться или заканчиваться дефисом
    /// </summary>
    public static Error SubdomainInvalidStartOrEnd =>
        Error.Validation(
            "Tenant.SubdomainInvalidStartOrEnd",
            "Subdomain cannot start or end with a hyphen.");

    /// <summary>
    /// Невалидный email администратора
    /// </summary>
    public static Error InvalidAdminEmail =>
        Error.Validation(
            "Tenant.InvalidAdminEmail",
            "Invalid administrator email format.");

    /// <summary>
    /// Тенант уже удален
    /// </summary>
    public static Error AlreadyDeleted =>
        Error.Conflict(
            "Tenant.AlreadyDeleted",
            "Tenant is already deleted.");

    /// <summary>
    /// Тенант приостановлен
    /// </summary>
    public static Error Suspended =>
        Error.Forbidden(
            "Tenant.Suspended",
            "Tenant is suspended and operations are not allowed.");

    /// <summary>
    /// Невозможно изменить статус тенанта
    /// </summary>
    public static Error CannotChangeStatus =>
        Error.Validation(
            "Tenant.CannotChangeStatus",
            "Cannot change tenant status.");
}

