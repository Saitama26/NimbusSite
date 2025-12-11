using Common.Infrastructure.Extensions;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Extensions;
using Tenants.Infrastructure.Extensions;
using Tenants.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Load .env if present (optional, local dev)
// Try loading from Tenants.Api folder first, then from root
var envPath = Path.Combine(AppContext.BaseDirectory, ".env");
if (!File.Exists(envPath))
{
    envPath = ".env";
}
Env.Load(envPath);

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
            // Не падаем, если миграции не применились - возможно БД еще не готова
        }
    }
}

// Swagger - всегда включаем
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenants API v1");
    c.RoutePrefix = string.Empty; // Swagger UI будет доступен по корневому пути
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault(); 
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

