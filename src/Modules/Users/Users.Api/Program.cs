using Common.Application.Abstractions.Events;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Tenants.Contracts.Events;
using Users.Application.Extensions;
using Users.Infrastructure;
using Users.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

ProjectRootHelper.LoadEnvFromProjectRoot();

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Users API",
        Version = "v1",
        Description = "API for user management. Provides endpoints for creating, updating, deleting users, and managing user status. All operations are scoped to a specific tenant."
    });

    // Включаем XML комментарии для Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.UseInlineDefinitionsForEnums();
});

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration)
    .AddCommonInfrastructure(builder.Configuration, typeof(Users.Application.Commands.CreateUser.CreateUserCommand).Assembly);

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("[Users Startup] Environment: {Env}", builder.Environment.EnvironmentName);

// Initialize tenant connection cache
try
{
    await app.Services.InitializeTenantCacheAsync();
    startupLogger.LogInformation("[Users Startup] Tenant cache initialized");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "[Users Startup] Failed to initialize tenant cache");
    throw;
}

// Subscribe to tenant events for cache updates
{
    var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();
    var cache = app.Services.GetRequiredService<TenantConnectionCache>();

    await eventSubscriber.SubscribeAsync<TenantCreatedEvent>(
        (evt, ct) =>
        {
            if (!string.IsNullOrEmpty(evt.ConnectionString))
            {
                cache.SetConnectionString(evt.TenantInt, evt.ConnectionString);
                startupLogger.LogInformation("Cache updated: added tenant {TenantInt}", evt.TenantInt);
            }
            return Task.CompletedTask;
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<TenantConnectionStringUpdatedEvent>(
        (evt, ct) =>
        {
            if (!string.IsNullOrEmpty(evt.ConnectionString))
            {
                cache.SetConnectionString(evt.TenantInt, evt.ConnectionString);
                startupLogger.LogInformation("Cache updated: connection string for tenant {TenantInt}", evt.TenantInt);
            }
            return Task.CompletedTask;
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<TenantDeletedEvent>(
        (evt, ct) =>
        {
            cache.RemoveConnectionString(evt.TenantInt);
            startupLogger.LogInformation("Cache updated: removed tenant {TenantInt}", evt.TenantInt);
            return Task.CompletedTask;
        }, CancellationToken.None);

    startupLogger.LogInformation("[Users Startup] Subscribed to tenant events");
}

// Apply migrations in Development
if (app.Environment.IsDevelopment())
{
    var connectionString = Environment.GetEnvironmentVariable("USERS_DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    opts => opts.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore))
                .Options;

            using var dbContext = new UsersDbContext(options);
            if (dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
                startupLogger.LogInformation("[Users Startup] Migrations applied");
            }
            else
            {
                dbContext.Database.EnsureCreated();
                startupLogger.LogInformation("[Users Startup] EnsureCreated executed");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "[Users Startup] Failed to apply migrations");
        }
    }
}

// TenantContext должен быть вызван ДО Swagger, чтобы иметь возможность читать тело запроса
app.UseTenantContext();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
