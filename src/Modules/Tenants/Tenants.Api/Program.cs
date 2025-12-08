using Common.Infrastructure.Extensions;
using DotNetEnv;
using Tenants.Application.Extensions;
using Tenants.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load .env if present (optional, local dev)
Env.Load(".env");

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

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tenants API v1");
    c.RoutePrefix = string.Empty; // Swagger UI будет доступен по корневому пути
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault(); // Включаем кнопку "Try it out" по умолчанию
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

