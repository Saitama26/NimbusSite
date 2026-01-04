using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application.Extensions;

/// <summary>
/// Расширения для регистрации сервисов Application слоя в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить Identity Application сервисы
    /// </summary>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // Event handlers регистрируются в Infrastructure слое
        return services;
    }
}

