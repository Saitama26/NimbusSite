using Microsoft.Extensions.DependencyInjection;

namespace Projects.Application.Extensions;

/// <summary>
/// DI-расширения для Projects Application
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        // Event handlers регистрируются в Infrastructure слое
        return services;
    }
}

