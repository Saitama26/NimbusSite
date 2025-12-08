using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для обработки CorrelationId
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Извлекаем CorrelationId из заголовка или генерируем новый
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeaderName] = correlationId;
        }

        // Добавляем в HttpContext.Items для использования в обработчиках
        context.Items[CorrelationIdItemKey] = correlationId.ToString();

        // Добавляем в заголовки ответа
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Добавляем в Scope для логирования
        using (_logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId.ToString()! } }))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// Получить CorrelationId из HttpContext
    /// </summary>
    public static string? GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdItemKey, out var value)
            ? value?.ToString()
            : null;
    }
}
