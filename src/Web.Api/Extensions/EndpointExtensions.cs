using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var descriptors = assembly
            .DefinedTypes
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpoint)))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
            .ToArray();

        services.TryAddEnumerable(descriptors);
        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app, IEndpointRouteBuilder? routeBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        var builder = routeBuilder ?? app;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}

