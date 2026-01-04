using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Extensions;
using Tenants.Contracts.Events;
using Tenants.Infrastructure.Extensions;
using Tenants.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта (только для локальной разработки)
// В CI/CD окружениях переменные передаются через Environment Variables
// Если .env файл отсутствует, используются системные Environment Variables
ProjectRootHelper.LoadEnvFromProjectRoot();

// Настраиваем порядок считывания конфигурации (как в AGSR)
// Порядок приоритета (от низкого к высокому):
// 1. appsettings.json
// 2. appsettings.{Environment}.json
// 3. User Secrets (только в Development)
// 4. Environment Variables (включая переменные из .env, если загружен)
// 5. Command Line Arguments
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Environment Variables имеют приоритет над appsettings

// Логируем, что видим в окружении/конфиге для TENANTS_DB_CONNECTION_STRING
var envConn = Environment.GetEnvironmentVariable("TENANTS_DB_CONNECTION_STRING");
var cfgConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
Console.WriteLine($"[Tenants Startup] TENANTS_DB_CONNECTION_STRING: {envConn ?? "(null)"}");
Console.WriteLine($"[Tenants Startup] DefaultConnection (config): {cfgConn ?? "(null)"}");

// Add services
builder.Services
    .AddTenantsApplication()
    .AddTenantsInfrastructure(builder.Configuration)
    .AddCommonInfrastructure(builder.Configuration, typeof(Tenants.Application.Commands.CreateTenant.CreateTenantCommand).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Tenants API",
        Version = "v1",
        Description = "API для управления тенантами в системе NimbusSite. Provides endpoints for creating, updating, deleting tenants, managing tenant connection strings, and changing tenant status. This API manages the central tenant registry."
    });

    // Включаем XML комментарии для Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Используем строковые значения для enum в Swagger
    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// Применяем миграции при старте (только в Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            // Ждем, пока БД станет доступной (максимум 3 минуты)
            var maxRetries = 180;
            var retryDelay = TimeSpan.FromSeconds(1);
            var connected = false;
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (await dbContext.Database.CanConnectAsync())
                    {
                        connected = true;
                        break;
                    }
                }
                catch
                {
                    // Игнорируем ошибки подключения и пробуем снова
                }
                
                logger.LogInformation("Waiting for database to be ready... ({Attempt}/{MaxRetries})", i + 1, maxRetries);
                await Task.Delay(retryDelay);
            }
            
            if (!connected)
            {
                logger.LogWarning("Database is not available after {MaxRetries} attempts. Creating database...", maxRetries);
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully.");
            }
            else
            {
                // Если БД существует, применяем миграции (если они есть)
                logger.LogInformation("Database is ready. Applying migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database. Application will continue anyway.");
            // Не прерываем запуск приложения, если миграции не удались
        }
    }
}

// Swagger - всегда включаем
app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenants API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault(); 
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

// Подписываемся на события после построения приложения
using (var scope = app.Services.CreateScope())
{
    var eventSubscriber = scope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventSubscriber>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Подписываемся на TenantCreatedEvent
    await eventSubscriber.SubscribeAsync<TenantCreatedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<TenantCreatedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    logger.LogInformation("Tenants event handlers subscribed successfully");
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

