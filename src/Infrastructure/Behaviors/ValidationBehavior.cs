using Application.Abstractions.Messaging;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Reflection;

namespace Infrastructure.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));

            return CreateValidationResult<TResponse>(failures);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(List<FluentValidation.Results.ValidationFailure> failures)
        where TResult : Result
    {
        var error = Error.Validation(
            "ValidationError",
            string.Join(", ", failures.Select(f => f.ErrorMessage)));

        // Используем implicit operator для преобразования Error в Result или Result<T>
        if (typeof(TResult) == typeof(Result))
        {
            return (TResult)(object)Result.Failure(error);
        }

        // Для Result<TResponse> используем reflection для создания через private constructor
        var resultType = typeof(TResult).GenericTypeArguments[0];
        var genericResultType = typeof(Result<>).MakeGenericType(resultType);
        
        var constructor = genericResultType.GetConstructors(
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 1 && 
                                 c.GetParameters()[0].ParameterType == typeof(Error));
        
        if (constructor != null)
        {
            return (TResult)constructor.Invoke(new object[] { error });
        }

        throw new InvalidOperationException($"Cannot create validation result for type {typeof(TResult)}");
    }
}

