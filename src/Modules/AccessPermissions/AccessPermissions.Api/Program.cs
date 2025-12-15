using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using AccessPermissions.Application.Extensions;
using AccessPermissions.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using AccessPermissions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта
ProjectRootHelper.LoadEnvFromProjectRoot();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "AccessPermissions API",
        Version = "v1",
        Description = "API для управления правами доступа"
    });
    c.UseInlineDefinitionsForEnums();
});

// Add Common Infrastructure
builder.Services.AddCommonInfrastructure(builder.Configuration, typeof(AccessPermissions.Application.Commands.CreateAccessPermission.CreateAccessPermissionCommand).Assembly);

// Add AccessPermissions Application and Infrastructure
builder.Services.AddAccessPermissionsApplication();
builder.Services.AddAccessPermissionsInfrastructure(builder.Configuration);

var app = builder.Build();

// Применяем миграции при старте (только в Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AccessPermissionsDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AccessPermissions API v1");
    c.RoutePrefix = string.Empty;
});

// Подписки на интеграционные события
using (var scope = app.Services.CreateScope())
{
    var eventSubscriber = scope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventSubscriber>();

    // Users
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserDeletedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserDeletedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Projects
    await eventSubscriber.SubscribeAsync<Contracts.Projects.Events.ProjectDeletedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Projects.Events.ProjectDeletedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Tasks
    await eventSubscriber.SubscribeAsync<Contracts.Tasks.Events.TaskDeletedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Tasks.Events.TaskDeletedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Tenants
    await eventSubscriber.SubscribeAsync<Contracts.Tenants.Events.TenantDeletedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Tenants.Events.TenantDeletedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

