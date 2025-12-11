using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Mappings;

namespace Projects.Application.Extensions;

/// <summary>
/// DI-расширения для Projects Application.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ProjectMappingProfile));
        return services;
    }
}

