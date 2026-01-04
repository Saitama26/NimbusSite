using Common.Application.Abstractions.Events;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Tenancy;
using Identity.Application.Extensions;
using Identity.Infrastructure;
using Identity.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Tenants.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION_STRING")))
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
        Title = "Identity API",
        Version = "v1",
        Description = "API for identity and authentication. Provides endpoints for user login, token refresh, password management, and session management."
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

builder.Services.AddCommonInfrastructure(builder.Configuration, typeof(Identity.Application.Commands.Login.LoginCommand).Assembly);
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("[Identity Startup] Environment: {Env}", builder.Environment.EnvironmentName);

try
{
    await app.Services.InitializeTenantCacheAsync();
    startupLogger.LogInformation("[Identity Startup] Tenant cache initialized");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "[Identity Startup] Failed to initialize tenant cache");
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

// Subscribe to User events
{
    var eventSubscriber = app.Services.GetRequiredService<IEventSubscriber>();

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserCreatedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserCreatedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserRegisteredEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserRegisteredEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserStatusChangedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserStatusChangedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserUpdatedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserUpdatedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    await eventSubscriber.SubscribeAsync<Users.Contracts.Events.UserDeletedEvent>(
        async (evt, ct) =>
        {
            using var scope = app.Services.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<Users.Contracts.Events.UserDeletedEvent>>();
            await handler.Handle(evt, ct);
        }, CancellationToken.None);

    startupLogger.LogInformation("[Identity Startup] Subscribed to events");
}

if (app.Environment.IsDevelopment())
{
    var connectionString = Environment.GetEnvironmentVariable("IDENTITY_DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    opts => opts.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore))
                .Options;

            using var dbContext = new IdentityDbContext(options);
            if (dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
                startupLogger.LogInformation("[Identity Startup] Migrations applied");
            }
            else
            {
                dbContext.Database.EnsureCreated();
                startupLogger.LogInformation("[Identity Startup] EnsureCreated executed");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "[Identity Startup] Failed to apply migrations");
        }
    }
}

// TenantContext должен быть вызван ДО Swagger, чтобы иметь возможность читать тело запроса
app.UseTenantContext();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
