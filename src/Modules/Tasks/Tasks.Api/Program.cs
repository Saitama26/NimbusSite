using Common.Application.Abstractions.Events;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Extensions;
using Tasks.Infrastructure;
using Tasks.Infrastructure.Extensions;
using Tenants.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TASKS_DB_CONNECTION_STRING")))
{
    ProjectRootHelper.LoadEnvFromProjectRoot();
}

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddTasksApplication()
    .AddTasksInfrastructure(builder.Configuration)
    .AddCommonInfrastructure(builder.Configuration, typeof(Tasks.Application.Commands.CreateTask.CreateTaskCommand).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Tasks API",
        Version = "v1",
        Description = "API for task management. Provides endpoints for creating, updating, deleting tasks, assigning tasks to users, and changing task status. All operations are scoped to a specific tenant."
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

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("[Tasks Startup] Environment: {Env}", builder.Environment.EnvironmentName);

try
{
    await app.Services.InitializeTenantCacheAsync();
    startupLogger.LogInformation("[Tasks Startup] Tenant cache initialized");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "[Tasks Startup] Failed to initialize tenant cache");
    throw;
}

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

    startupLogger.LogInformation("[Tasks Startup] Subscribed to tenant events");
}

if (app.Environment.IsDevelopment())
{
    var connectionString = Environment.GetEnvironmentVariable("TASKS_DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var options = new DbContextOptionsBuilder<TasksDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    opts => opts.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore))
                .Options;

            using var dbContext = new TasksDbContext(options);
            if (dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
                startupLogger.LogInformation("[Tasks Startup] Migrations applied");
            }
            else
            {
                dbContext.Database.EnsureCreated();
                startupLogger.LogInformation("[Tasks Startup] EnsureCreated executed");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "[Tasks Startup] Failed to apply migrations");
        }
    }
}

// TenantContext должен быть вызван ДО Swagger, чтобы иметь возможность читать тело запроса
app.UseTenantContext();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
