using Common.Application.Abstractions.Events;
using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Common.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Extensions;
using Projects.Infrastructure;
using Projects.Infrastructure.Extensions;
using Tenants.Contracts.Events;

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PROJECTS_DB_CONNECTION_STRING")))
{
    ProjectRootHelper.LoadEnvFromProjectRoot();
}

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services
    .AddProjectsApplication()
    .AddProjectsInfrastructure(builder.Configuration)
    .AddCommonInfrastructure(builder.Configuration, typeof(Projects.Application.Commands.CreateProject.CreateProjectCommand).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Projects API",
        Version = "v1",
        Description = "API for project management. Provides endpoints for creating, updating, deleting projects, managing project users, and changing project status. All operations are scoped to a specific tenant."
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
startupLogger.LogInformation("[Projects Startup] Environment: {Env}", builder.Environment.EnvironmentName);

// Initialize tenant connection cache
try
{
    await app.Services.InitializeTenantCacheAsync();
    startupLogger.LogInformation("[Projects Startup] Tenant cache initialized");
}
catch (Exception ex)
{
    startupLogger.LogError(ex, "[Projects Startup] Failed to initialize tenant cache");
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

    startupLogger.LogInformation("[Projects Startup] Subscribed to tenant events");
}

// Apply migrations in Development
if (app.Environment.IsDevelopment())
{
    var connectionString = Environment.GetEnvironmentVariable("PROJECTS_DB_CONNECTION_STRING");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            var options = new DbContextOptionsBuilder<ProjectsDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    opts => opts.SchemaBehavior(Pomelo.EntityFrameworkCore.MySql.Infrastructure.MySqlSchemaBehavior.Ignore))
                .Options;

            using var dbContext = new ProjectsDbContext(options);
            if (dbContext.Database.GetMigrations().Any())
            {
                dbContext.Database.Migrate();
                startupLogger.LogInformation("[Projects Startup] Migrations applied");
            }
            else
            {
                dbContext.Database.EnsureCreated();
                startupLogger.LogInformation("[Projects Startup] EnsureCreated executed");
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "[Projects Startup] Failed to apply migrations");
        }
    }
}

// TenantContext должен быть вызван ДО Swagger, чтобы иметь возможность читать тело запроса
app.UseTenantContext();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Projects API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
