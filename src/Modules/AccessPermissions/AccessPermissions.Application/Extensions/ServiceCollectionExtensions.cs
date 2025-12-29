using Microsoft.Extensions.DependencyInjection;

namespace AccessPermissions.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить AccessPermissions Application сервисы
    /// </summary>
    public static IServiceCollection AddAccessPermissionsApplication(this IServiceCollection services)
    {
        // Event handlers регистрируются в Infrastructure слое
        return services;
    }
}

