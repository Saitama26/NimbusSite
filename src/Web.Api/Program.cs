using Application;
using Infrastructure;
using Infrastructure.Behaviors;
using Microsoft.OpenApi.Models;

// Загружаем переменные окружения из .env файла
// Путь относительно Web.Api проекта: ../NimbusSite/.env
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else
{
    // Пробуем альтернативный путь
    var altPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
    if (File.Exists(altPath))
    {
        DotNetEnv.Env.Load(altPath);
    }
}

var builder = WebApplication.CreateBuilder(args);

// Добавляем переменные окружения в конфигурацию
builder.Configuration.AddEnvironmentVariables();

// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Application Layer
builder.Services.AddApplication();

// Infrastructure Layer
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR и Behaviors
var applicationAssembly = typeof(Application.AssemblyReference).Assembly;
builder.Services.AddMediatRBehaviors(applicationAssembly);

// CORS
var corsAllowedOriginsEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
var allowedOrigins = !string.IsNullOrWhiteSpace(corsAllowedOriginsEnv)
    ? corsAllowedOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:3000", "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Controllers (optional, endpoints are used by default)
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Включаем аннотации Swagger
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "NimbusSite API",
        Description = """
            NimbusSite - Multi-tenant Project Management System API
            
            ## Features
            - **Multi-tenant Architecture**: Complete tenant isolation with sharding support
            - **JWT Authentication**: Secure token-based authentication with refresh tokens
            - **CQRS Pattern**: Separation of commands and queries for better scalability
            - **Domain Events**: Event-driven architecture for decoupled components
            - **Clean Architecture**: Layered architecture with clear separation of concerns
            
            ## Error Handling
            All errors follow a consistent format:
            {
              "error": {
                "code": "ErrorCode",
                "description": "Human-readable error message",
                "type": "Validation|NotFound|Unauthorized|Failure|Conflict"
              }
            }
            """,
    });

    // JWT security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Уникальные schemaIds
    options.CustomSchemaIds(type =>
    {
        var full = type.FullName ?? type.Name;
        return full.Replace(".", "_").Replace("+", "_");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger только для Development окружения
if (app.Environment.IsDevelopment())
{
    // Обрабатываем ошибки Swagger, чтобы они не блокировали API
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            try
            {
                await next();
            }
            catch (Exception ex) when (ex.Message.Contains("schemaId") || ex.Message.Contains("Swagger"))
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Swagger Error: {ex.Message}. API endpoints are still available.");
            }
        }
        else
        {
            await next();
        }
    });
    
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "NimbusSite API v1");
        options.RoutePrefix = "swagger"; // Swagger UI доступен по /swagger
        options.DocumentTitle = "NimbusSite API Documentation";
        options.DefaultModelsExpandDepth(-1); // Скрываем модели по умолчанию
        options.EnableDeepLinking(); // Включаем глубокие ссылки
        options.EnableFilter(); // Включаем фильтрацию
        options.ShowExtensions(); // Показываем расширения
        options.EnableValidator(); // Включаем валидатор
        options.DisplayRequestDuration(); // Показываем время выполнения запроса
        options.EnableTryItOutByDefault(); // Включаем "Try it out" по умолчанию
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();