using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Extensions;
using Tenants.Infrastructure.Extensions;
using Tenants.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта
ProjectRootHelper.LoadEnvFromProjectRoot();

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
        Description = "API для управления тенантами в системе NimbusSite"
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
    await eventSubscriber.SubscribeAsync<Contracts.Tenants.Events.TenantCreatedEvent>(
        async (evt, ct) =>
        {
            using var handlerScope = app.Services.CreateScope();
            var handler = handlerScope.ServiceProvider.GetRequiredService<Common.Application.Abstractions.Events.IEventHandler<Contracts.Tenants.Events.TenantCreatedEvent>>();
            await handler.Handle(evt, ct);
        },
        CancellationToken.None);

    logger.LogInformation("Tenants event handlers subscribed successfully");
}

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

