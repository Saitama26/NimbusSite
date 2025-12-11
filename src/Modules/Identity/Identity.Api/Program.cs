using Common.Infrastructure.Extensions;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env if present
var envCandidates = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), "src", "Modules", "Identity", "Identity.Api", ".env"),
    Path.Combine(AppContext.BaseDirectory, ".env"),
    ".env"
};

foreach (var path in envCandidates)
{
    if (File.Exists(path))
    {
        Env.Load(path);
        break;
    }
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1",
        Description = "API для управления идентификацией и аутентификацией"
    });
    c.UseInlineDefinitionsForEnums();
});

// Add Common Infrastructure (without module-specific services for now)
builder.Services.AddCommonInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

