using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для определения TenantId из JWT токена или заголовка
/// </summary>
public sealed class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantContextMiddleware> _logger;
    private const string TenantIdHeaderName = "X-Tenant-ID";
    private const string TenantIdItemKey = "TenantId";
    private const string TenantIdClaimName = "TenantId";

    public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Guid? tenantId = null;

        // Пытаемся извлечь из JWT токена (если пользователь авторизован)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst(TenantIdClaimName);
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var parsedTenantId))
            {
                tenantId = parsedTenantId;
            }
        }

        // Если не нашли в токене, пытаемся из заголовка
        if (!tenantId.HasValue && context.Request.Headers.TryGetValue(TenantIdHeaderName, out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader.ToString(), out var headerTenantId))
            {
                tenantId = headerTenantId;
            }
        }

        // Сохраняем в HttpContext.Items
        if (tenantId.HasValue)
        {
            context.Items[TenantIdItemKey] = tenantId.Value;
        }

        await _next(context);
    }

    /// <summary>
    /// Получить TenantId из HttpContext
    /// </summary>
    public static Guid? GetTenantId(HttpContext context)
    {
        return context.Items.TryGetValue(TenantIdItemKey, out var value) && value is Guid tenantId
            ? tenantId
            : null;
    }
}
