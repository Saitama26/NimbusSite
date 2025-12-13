using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Identity.Application.Extensions;
using Identity.Infrastructure.Extensions;
using Users.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта
ProjectRootHelper.LoadEnvFromProjectRoot();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1",
        Description = "API для управления идентификацией и аутентификацией"
    });
    c.UseInlineDefinitionsForEnums();
});

// Add Common Infrastructure
builder.Services.AddCommonInfrastructure(builder.Configuration, typeof(Identity.Application.Commands.Login.LoginCommand).Assembly);

// Add Identity Application and Infrastructure
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// Add Users Infrastructure (для доступа к IUserRepository в LoginCommand)
builder.Services.AddUsersInfrastructure(builder.Configuration);

var app = builder.Build();

// Подписываемся на события после построения приложения
using (var scope = app.Services.CreateScope())
{
    var eventSubscriber = scope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventSubscriber>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    // Подписываемся на UserCreatedEvent
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserCreatedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserCreatedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Подписываемся на UserRegisteredEvent
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserRegisteredEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserRegisteredEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Подписываемся на UserStatusChangedEvent
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserStatusChangedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserStatusChangedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Подписываемся на UserUpdatedEvent
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserUpdatedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserUpdatedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    // Подписываемся на UserDeletedEvent
    await eventSubscriber.SubscribeAsync<Contracts.Users.Events.UserDeletedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Users.Events.UserDeletedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    logger.LogInformation("Identity event handlers subscribed successfully");
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

