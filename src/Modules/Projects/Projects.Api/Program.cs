using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Projects.Application.Extensions;
using Projects.Infrastructure;
using Projects.Infrastructure.Extensions;
using Tenants.Infrastructure.Extensions;
using Users.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта
ProjectRootHelper.LoadEnvFromProjectRoot();

// Services
builder.Services
    .AddProjectsApplication()
    .AddProjectsInfrastructure(builder.Configuration)
    .AddTenantsInfrastructure(builder.Configuration) 
    .AddUsersInfrastructure(builder.Configuration) 
    .AddCommonInfrastructure(
        builder.Configuration, 
        typeof(Projects.Application.Commands.CreateProject.CreateProjectCommand).Assembly,
        typeof(Tenants.Application.Commands.CreateTenant.CreateTenantCommand).Assembly); 

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Projects API",
        Version = "v1",
        Description = "API для управления проектами"
    });

    // Включаем XML комментарии из API проекта
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Включаем XML комментарии из Application проекта
    var applicationAssembly = typeof(Projects.Application.Commands.CreateProject.CreateProjectCommand).Assembly;
    var applicationXmlFile = $"{applicationAssembly.GetName().Name}.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        c.IncludeXmlComments(applicationXmlPath);
    }

    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// Применяем миграции при старте (только в Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
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

app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Projects API v1");
    c.RoutePrefix = string.Empty;
    c.DisplayRequestDuration();
    c.EnableTryItOutByDefault();
    c.EnableDeepLinking();
    c.EnableFilter();
    c.ShowExtensions();
});

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();