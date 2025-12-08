using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using MediatR;

namespace Common.Infrastructure.Behaviors;

/// <summary>
/// Behavior для валидации команд перед выполнением
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
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
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            var errorMessages = failures
                .Select(f => f.ErrorMessage)
                .ToList();

            var error = Error.Validation(
                "ValidationError",
                string.Join("; ", errorMessages));

            // Создаем Result.Failure динамически
            var failureResultType = typeof(Result<>).MakeGenericType(typeof(TResponse).GenericTypeArguments);
            var failureMethod = typeof(Result<>).MakeGenericType(typeof(TResponse).GenericTypeArguments)
                .GetMethod("Failure", new[] { typeof(Error) });

            if (failureMethod != null)
            {
                var failureResult = failureMethod.Invoke(null, new object[] { error });
                return (TResponse)failureResult!;
            }

            // Fallback - создаем через рефлексию
            return (TResponse)Activator.CreateInstance(typeof(TResponse), false, default, error)!;
        }

        return await next();
    }
}
