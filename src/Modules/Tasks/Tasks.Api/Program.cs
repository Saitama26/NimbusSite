using Common.Infrastructure.Configuration;
using Common.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Tasks.Application.Extensions;
using Tasks.Infrastructure;
using Tasks.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Загружаем .env из корня проекта
ProjectRootHelper.LoadEnvFromProjectRoot();

// Services
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
        Description = "API для управления задачами"
    });

    // Включаем XML комментарии из API проекта
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Включаем XML комментарии из Application проекта
    var applicationAssembly = typeof(Tasks.Application.Commands.CreateTask.CreateTaskCommand).Assembly;
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
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
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

app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0);
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasks API v1");
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

