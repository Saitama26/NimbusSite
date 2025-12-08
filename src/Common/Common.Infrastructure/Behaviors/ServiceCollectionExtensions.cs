using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Infrastructure.Behaviors;

/// <summary>
/// Расширения для регистрации behaviors в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить behaviors для обработки команд и запросов
    /// </summary>
    public static IServiceCollection AddBehaviors(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        // Регистрируем FluentValidation валидаторы
        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        // Регистрируем MediatR behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
