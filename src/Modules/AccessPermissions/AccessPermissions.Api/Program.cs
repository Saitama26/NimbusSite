using Common.Application.Abstractions.Events;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Tenancy;
using AccessPermissions.Application.Extensions;
using AccessPermissions.Infrastructure;
using AccessPermissions.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Tenants.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ACCESSPERMISSIONS_DB_CONNECTION_STRING")))
{
    ProjectRootHelper.LoadEnvFromProjectRoot();
}

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
        Title = "AccessPermissions API",
        Version = "v1",
        Description = "API for access permissions management. Provides endpoints for creating, updating, deleting access permissions, and querying user permissions. All operations are scoped to a specific tenant."
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

builder.Services.AddCommonInfrastructure(builder.Configuration, typeof(AccessPermissions.Application.Commands.CreateAccessPermission.CreateAccessPermissionCommand).Assembly);
builder.Services.AddAccessPermissionsApplication();
builder.Services.AddAccessPermissionsInfrastructure(builder.Configuration);

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("[AccessPermissions Startup] Environment: {Env}", builder.Environment.EnvironmentName);

try
{
    await app.Services.InitializeTenantCacheAsync();
    startupLogger.LogInformation("[AccessPermissions Startup] Tenant cache initialized");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "[AccessPermissions Startup] Failed to initialize tenant cache");
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
                cache.SetConnectionString(evt.TenantInt, evt.ConnectionString);
            return Task.CompletedTask;
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<TenantConnectionStringUpdatedEvent>(
        (evt, ct) =>
        {
            if (!string.IsNullOrEmpty(evt.ConnectionString))
                cache.SetConnectionString(evt.TenantInt, evt.ConnectionString);
            return Task.CompletedTask;
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<TenantDeletedEvent>(
        (evt, ct) =>
        {
            cache.RemoveConnectionString(evt.TenantInt);
            return Task.CompletedTask;
        }, CancellationToken.None);
}

// Subscribe to domain events
{
    var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserDeletedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserDeletedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Projects.Contracts.Events.ProjectDeletedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Projects.Contracts.Events.ProjectDeletedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Tasks.Contracts.Events.TaskDeletedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Tasks.Contracts.Events.TaskDeletedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<TenantDeletedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<TenantDeletedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    startupLogger.LogInformation("[AccessPermissions Startup] Subscribed to events");
}

if (app.Environment.IsDevelopment())
{
    var connectionString = Environment.GetEnvironmentVariable("ACCESSPERMISSIONS_DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var options = new DbContextOptionsBuilder<AccessPermissionsDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    opts => opts.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore))
                .Options;

            using var dbContext = new AccessPermissionsDbContext(options);
            if (dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
                startupLogger.LogInformation("[AccessPermissions Startup] Migrations applied");
            }
            else
            {
                dbContext.Database.EnsureCreated();
                startupLogger.LogInformation("[AccessPermissions Startup] EnsureCreated executed");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "[AccessPermissions Startup] Failed to apply migrations");
        }
    }
}

// TenantContext должен быть вызван ДО Swagger, чтобы иметь возможность читать тело запроса
app.UseTenantContext();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AccessPermissions API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
