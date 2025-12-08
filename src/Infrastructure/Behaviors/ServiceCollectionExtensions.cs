using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Linq;

namespace Infrastructure.Behaviors;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRBehaviors(
        this IServiceCollection services,
        Assembly applicationAssembly)
    {
        // Регистрация MediatR с автоматическим сканированием handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(applicationAssembly);
        });
        
        // Явная регистрация обработчиков для гарантии их обнаружения
        // MediatR должен находить их автоматически, но добавляем явную регистрацию как fallback
        var handlerTypes = applicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsNested)
            .Where(t => 
            {
                // Проверяем, реализует ли класс ICommandHandler или IQueryHandler
                var interfaces = t.GetInterfaces();
                return interfaces.Any(i => 
                    i.IsGenericType && 
                    (i.GetGenericTypeDefinition().Name.StartsWith("ICommandHandler") ||
                     i.GetGenericTypeDefinition().Name.StartsWith("IQueryHandler")));
            })
            .ToList();
        
        // Регистрируем каждый обработчик напрямую через его интерфейсы
        foreach (var handlerType in handlerTypes)
        {
            // Находим все интерфейсы, которые наследуются от IRequestHandler
            var allInterfaces = handlerType.GetInterfaces();
            var requestHandlerInterfaces = new List<Type>();
            
            foreach (var interfaceType in allInterfaces)
            {
                // Проверяем напрямую
                if (interfaceType.IsGenericType && 
                    interfaceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                {
                    requestHandlerInterfaces.Add(interfaceType);
                }
                // Проверяем через базовые интерфейсы (ICommandHandler, IQueryHandler)
                else if (interfaceType.IsGenericType)
                {
                    var baseInterfaces = interfaceType.GetInterfaces();
                    foreach (var baseInterface in baseInterfaces)
                    {
                        if (baseInterface.IsGenericType && 
                            baseInterface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                        {
                            requestHandlerInterfaces.Add(baseInterface);
                        }
                    }
                }
            }
            
            // Регистрируем каждый найденный интерфейс
            foreach (var interfaceType in requestHandlerInterfaces.Distinct())
            {
                services.AddScoped(interfaceType, handlerType);
            }
        }

        // Регистрация FluentValidation валидаторов
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Регистрация pipeline behaviors в правильном порядке
        // Порядок важен: Validation -> Logging -> Transaction
        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(TransactionBehavior<,>));

        return services;
    }
}

