using Microsoft.Extensions.DependencyInjection;

namespace Tasks.Application.Extensions;

/// <summary>
/// DI-расширения для Tasks Application
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTasksApplication(this IServiceCollection services)
    {
        return services;
    }
}

