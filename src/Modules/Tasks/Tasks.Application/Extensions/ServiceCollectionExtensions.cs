using Microsoft.Extensions.DependencyInjection;
using Tasks.Application.Mappings;

namespace Tasks.Application.Extensions;

/// <summary>
/// DI-расширения для Tasks Application
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTasksApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(TaskMappingProfile));
        return services;
    }
}

