using Common.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Common.Infrastructure.Extensions;

/// <summary>
/// Расширения для конфигурации HTTP pipeline
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Использовать обработку ошибок
    /// </summary>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }

    /// <summary>
    /// Использовать CorrelationId middleware
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Использовать TenantContext middleware
    /// </summary>
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantContextMiddleware>();
    }
}
