using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Common.Infrastructure.Tenancy;

/// <summary>
/// Сервис для извлечения информации о тенанте из HTTP контекста
/// Поддерживает извлечение из JWT токена (claim "tenant") или query параметра
/// </summary>
public sealed class UserTenantService
{
    private readonly ILogger<UserTenantService> _logger;
    private const string TenantClaimName = "tenant";
    private const string TenantIntClaimName = "tenantInt";
    private const string TenantQueryParameterName = "tenant";
    private const string TenantIntQueryParameterName = "tenantId";
    private const string TenantItemKey = "TenantName";
    private const string TenantIntItemKey = "TenantInt";

    public UserTenantService(ILogger<UserTenantService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Извлечь имя тенанта из JWT токена
    /// </summary>
    public string? GetTenantFromJwt(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var tenantClaim = context.User.FindFirst(TenantClaimName);
        if (tenantClaim != null && !string.IsNullOrWhiteSpace(tenantClaim.Value))
        {
            _logger.LogDebug("Tenant name extracted from JWT: {TenantName}", tenantClaim.Value);
            return tenantClaim.Value;
        }

        return null;
    }

    /// <summary>
    /// Извлечь числовой идентификатор тенанта из JWT токена
    /// </summary>
    public int? GetTenantIntFromJwt(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var tenantIntClaim = context.User.FindFirst(TenantIntClaimName);
        if (tenantIntClaim != null && int.TryParse(tenantIntClaim.Value, out var tenantInt))
        {
            _logger.LogDebug("TenantInt extracted from JWT: {TenantInt}", tenantInt);
            return tenantInt;
        }

        return null;
    }

    /// <summary>
    /// Извлечь имя тенанта из query параметра
    /// </summary>
    public string? GetTenantFromQuery(HttpContext context)
    {
        if (context.Request.Query.TryGetValue(TenantQueryParameterName, out var tenantValue))
        {
            var tenantName = tenantValue.ToString();
            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                _logger.LogDebug("Tenant name extracted from query: {TenantName}", tenantName);
                return tenantName;
            }
        }

        return null;
    }

    /// <summary>
    /// Получить имя тенанта из контекста (JWT или query)
    /// </summary>
    public string? GetTenantName(HttpContext context)
    {
        // Сначала проверяем HttpContext.Items (может быть установлено middleware)
        if (context.Items.TryGetValue(TenantItemKey, out var cachedTenant) && cachedTenant is string tenantName)
        {
            return tenantName;
        }

        // Пытаемся извлечь из JWT
        var tenantFromJwt = GetTenantFromJwt(context);
        if (!string.IsNullOrWhiteSpace(tenantFromJwt))
        {
            context.Items[TenantItemKey] = tenantFromJwt;
            return tenantFromJwt;
        }

        // Пытаемся извлечь из query параметра
        var tenantFromQuery = GetTenantFromQuery(context);
        if (!string.IsNullOrWhiteSpace(tenantFromQuery))
        {
            context.Items[TenantItemKey] = tenantFromQuery;
            return tenantFromQuery;
        }

        // Проверяем локальное окружение
        var isLocalEnvironment = Environment.GetEnvironmentVariable("IsLocalEnvironment") == "true";
        if (isLocalEnvironment)
        {
            var localTenant = Environment.GetEnvironmentVariable("LocalTenant");
            if (!string.IsNullOrWhiteSpace(localTenant))
            {
                _logger.LogDebug("Using local tenant from environment: {TenantName}", localTenant);
                context.Items[TenantItemKey] = localTenant;
                return localTenant;
            }
        }

        return null;
    }

    /// <summary>
    /// Извлечь числовой идентификатор тенанта из заголовка X-Tenant-Id
    /// </summary>
    public int? GetTenantIntFromHeader(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValue))
        {
            if (int.TryParse(tenantIdValue.ToString(), out var tenantInt))
            {
                _logger.LogDebug("TenantInt extracted from header X-Tenant-Id: {TenantInt}", tenantInt);
                return tenantInt;
            }
        }

        return null;
    }

    /// <summary>
    /// Извлечь TenantInt из query параметров (tenantId или tenant)
    /// </summary>
    public int? GetTenantIntFromQuery(HttpContext context)
    {
        if (context.Request.Query.TryGetValue(TenantIntQueryParameterName, out var tenantIdValue))
        {
            if (int.TryParse(tenantIdValue.ToString(), out var tenantInt))
            {
                _logger.LogDebug("TenantInt extracted from query param '{Param}': {TenantInt}", TenantIntQueryParameterName, tenantInt);
                return tenantInt;
            }
        }

        // fallback: пробуем параметр "tenant" как число
        if (context.Request.Query.TryGetValue(TenantQueryParameterName, out var tenantValue))
        {
            if (int.TryParse(tenantValue.ToString(), out var tenantInt))
            {
                _logger.LogDebug("TenantInt extracted from query param '{Param}': {TenantInt}", TenantQueryParameterName, tenantInt);
                return tenantInt;
            }
        }

        return null;
    }

    /// <summary>
    /// Получить числовой идентификатор тенанта из контекста
    /// </summary>
    public int? GetTenantInt(HttpContext context)
    {
        // Сначала проверяем HttpContext.Items
        if (context.Items.TryGetValue(TenantIntItemKey, out var cachedTenantInt) && cachedTenantInt is int tenantInt)
        {
            return tenantInt;
        }

        // Пытаемся извлечь из заголовка X-Tenant-Id
        var tenantIntFromHeader = GetTenantIntFromHeader(context);
        if (tenantIntFromHeader.HasValue)
        {
            context.Items[TenantIntItemKey] = tenantIntFromHeader.Value;
            return tenantIntFromHeader.Value;
        }

        // Пытаемся извлечь из query параметров
        var tenantIntFromQuery = GetTenantIntFromQuery(context);
        if (tenantIntFromQuery.HasValue)
        {
            context.Items[TenantIntItemKey] = tenantIntFromQuery.Value;
            return tenantIntFromQuery.Value;
        }

        // Пытаемся извлечь из JWT
        var tenantIntFromJwt = GetTenantIntFromJwt(context);
        if (tenantIntFromJwt.HasValue)
        {
            context.Items[TenantIntItemKey] = tenantIntFromJwt.Value;
            return tenantIntFromJwt.Value;
        }

        return null;
    }
}

