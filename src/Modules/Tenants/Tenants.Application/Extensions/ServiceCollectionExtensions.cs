using Microsoft.Extensions.DependencyInjection;

namespace Tenants.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Tenants Application сервисы
    /// </summary>
    public static IServiceCollection AddTenantsApplication(this IServiceCollection services)
    {
        // Event handlers регистрируются в Infrastructure слое
        return services;
    }
}

