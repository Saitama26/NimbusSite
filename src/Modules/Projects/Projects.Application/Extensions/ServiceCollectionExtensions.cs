using Microsoft.Extensions.DependencyInjection;
using Projects.Application.Mappings;
using Common.Application.Abstractions.Events;
using Projects.Application.EventHandlers;
using Contracts.Users.Events;
using Contracts.Tenants.Events;

namespace Projects.Application.Extensions;

/// <summary>
/// DI-расширения для Projects Application.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ProjectMappingProfile));
        services.AddScoped<IEventHandler<UserDeletedEvent>, UserDeletedEventHandler>();
        services.AddScoped<IEventHandler<TenantDeletedEvent>, TenantDeletedEventHandler>();
        return services;
    }
}

