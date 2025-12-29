using Common.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для определения tenant name и tenantInt из JWT токена или query параметра
/// </summary>
public sealed class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UserTenantService _userTenantService;
    private readonly ILogger<TenantContextMiddleware> _logger;
    private const string TenantNameItemKey = "TenantName";
    private const string TenantIntItemKey = "TenantInt";

    public TenantContextMiddleware(
        RequestDelegate next,
        UserTenantService userTenantService,
        ILogger<TenantContextMiddleware> logger)
    {
        _next = next;
        _userTenantService = userTenantService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogDebug("TenantContextMiddleware invoked for {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        // Извлекаем tenant name из JWT или query
        var tenantName = _userTenantService.GetTenantName(context);
        if (!string.IsNullOrWhiteSpace(tenantName))
        {
            context.Items[TenantNameItemKey] = tenantName;
            _logger.LogDebug("Tenant name set in context: {TenantName}", tenantName);
        }

        // Извлекаем tenantInt из JWT, query параметров или заголовков
        var tenantInt = _userTenantService.GetTenantInt(context);
        _logger.LogDebug("TenantInt from headers/query/JWT: {TenantInt}", tenantInt);
        
        // Если tenantInt не найден, пробуем извлечь из тела запроса для POST запросов (например, Login)
        if (!tenantInt.HasValue && context.Request.Method == "POST" && 
            context.Request.ContentType?.Contains("application/json") == true)
        {
            _logger.LogDebug("Attempting to extract TenantInt from request body for POST request");
            tenantInt = await TryGetTenantIntFromBodyAsync(context);
            _logger.LogDebug("TenantInt from request body: {TenantInt}", tenantInt);
        }
        
        if (tenantInt.HasValue)
        {
            context.Items[TenantIntItemKey] = tenantInt.Value;
            _logger.LogInformation("TenantInt set in context: {TenantInt}", tenantInt.Value);
        }
        else
        {
            _logger.LogWarning("TenantInt not found in any source (headers, query, JWT, or body) for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }

        await _next(context);
    }

    /// <summary>
    /// Попытаться извлечь TenantId из тела запроса (для Login и подобных запросов)
    /// </summary>
    private async Task<int?> TryGetTenantIntFromBodyAsync(HttpContext context)
    {
        try
        {
            // Включаем буферизацию для возможности повторного чтения тела запроса
            // Это должно быть вызвано ДО того, как тело запроса будет прочитано ASP.NET Core
            // EnableBuffering() создает буферизованный поток, который можно читать несколько раз
            context.Request.EnableBuffering();
            
            // Проверяем, что поток поддерживает позиционирование
            if (!context.Request.Body.CanSeek)
            {
                _logger.LogWarning("Request body stream does not support seeking, cannot read body");
                return null;
            }
            
            // Сохраняем текущую позицию (обычно 0, если тело еще не читалось)
            var originalPosition = context.Request.Body.Position;
            var bodyLength = context.Request.Body.Length;
            _logger.LogDebug("Original body position: {Position}, CanSeek: {CanSeek}, CanRead: {CanRead}, Length: {Length}", 
                originalPosition, context.Request.Body.CanSeek, context.Request.Body.CanRead, bodyLength);
            
            // Если длина 0, тело пустое
            if (bodyLength == 0)
            {
                _logger.LogDebug("Request body is empty (length = 0)");
                return null;
            }
            
            // Устанавливаем позицию на начало для чтения
            context.Request.Body.Position = 0;
            
            // Читаем тело запроса
            using var reader = new System.IO.StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            _logger.LogDebug("Read body length: {Length}, Body preview: {Preview}", 
                body?.Length ?? 0, body?.Length > 0 ? body.Substring(0, Math.Min(200, body.Length)) : "");
            
            // Возвращаем позицию на начало для дальнейшего чтения контроллером
            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogDebug("Body is empty or whitespace");
                return null;
            }

            // Парсим JSON и ищем TenantId
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("TenantId", out var tenantIdElement))
            {
                if (tenantIdElement.ValueKind == System.Text.Json.JsonValueKind.Number && 
                    tenantIdElement.TryGetInt32(out var tenantInt))
                {
                    _logger.LogInformation("TenantInt extracted from request body: {TenantInt}", tenantInt);
                    return tenantInt;
                }
                else
                {
                    _logger.LogDebug("TenantId found in body but is not a number: {ValueKind}", tenantIdElement.ValueKind);
                }
            }
            else
            {
                _logger.LogDebug("TenantId property not found in request body. Available properties: {Properties}", 
                    string.Join(", ", jsonDoc.RootElement.EnumerateObject().Select(p => p.Name)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract TenantId from request body. Error: {Error}", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Получить имя тенанта из HttpContext
    /// </summary>
    public static string? GetTenantName(HttpContext context)
    {
        return context.Items.TryGetValue(TenantNameItemKey, out var value) && value is string tenantName
            ? tenantName
            : null;
    }

    /// <summary>
    /// Получить числовой идентификатор тенанта из HttpContext
    /// </summary>
    public static int? GetTenantInt(HttpContext context)
    {
        return context.Items.TryGetValue(TenantIntItemKey, out var value) && value is int tenantInt
            ? tenantInt
            : null;
    }
}
