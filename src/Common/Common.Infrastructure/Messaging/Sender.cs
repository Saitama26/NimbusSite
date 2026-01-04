using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Common.Infrastructure.Messaging;

/// <summary>
/// Реализация ISender без MediatR
/// Находит handlers через DI и выполняет валидацию и логирование
/// </summary>
public sealed class Sender : ISender
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Sender> _logger;

    public Sender(IServiceProvider serviceProvider, ILogger<Sender> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result> Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var requestName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            // Валидация
            var validationResult = await ValidateAsync(command, cancellationToken);
            if (validationResult != null)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "Validation failed for {RequestName}: {ErrorDescription}",
                    requestName,
                    validationResult.Error?.Description);
                return validationResult;
            }

            // Поиск handler
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                stopwatch.Stop();
                var error = Error.Failure("HandlerNotFound", $"Handler for {requestName} not found");
                _logger.LogError("Handler not found for {RequestName}", requestName);
                return Result.Failure(error);
            }

            // Выполнение handler через рефлексию
            var handleMethod = handlerType.GetMethod("Handle")!;
            var resultTask = (Task<Result>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
            var result = await resultTask;

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Handled {RequestName} with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    requestName,
                    result.Error?.Code,
                    result.Error?.Description,
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<Result<TResult>> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var requestName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            // Валидация
            var validationResult = await ValidateAsync<TCommand, TResult>(command, cancellationToken);
            if (validationResult != null)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "Validation failed for {RequestName}: {ErrorDescription}",
                    requestName,
                    validationResult.Error?.Description);
                return validationResult;
            }

            // Поиск handler
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(TResult));
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                stopwatch.Stop();
                var error = Error.Failure("HandlerNotFound", $"Handler for {requestName} not found");
                _logger.LogError("Handler not found for {RequestName}", requestName);
                return Result<TResult>.Failure(error);
            }

            // Выполнение handler через рефлексию
            var handleMethod = handlerType.GetMethod("Handle")!;
            var resultTask = (Task<Result<TResult>>)handleMethod.Invoke(handler, new object[] { command, cancellationToken })!;
            var result = await resultTask;

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Handled {RequestName} with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    requestName,
                    result.Error?.Code,
                    result.Error?.Description,
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<Result<TResult>> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var requestName = query.GetType().Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            // Валидация
            var validationResult = await ValidateQueryAsync(query, cancellationToken);
            if (validationResult != null)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    "Validation failed for {RequestName}: {ErrorDescription}",
                    requestName,
                    validationResult.Error?.Description);
                return validationResult;
            }

            // Поиск handler через рефлексию
            var queryType = query.GetType();
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
            var handler = _serviceProvider.GetService(handlerType);
            
            if (handler == null)
            {
                stopwatch.Stop();
                var error = Error.Failure("HandlerNotFound", $"Handler for {requestName} not found");
                _logger.LogError("Handler not found for {RequestName}", requestName);
                return Result<TResult>.Failure(error);
            }

            // Выполнение handler через рефлексию
            var handleMethod = handlerType.GetMethod("Handle")!;
            var resultTask = (Task<Result<TResult>>)handleMethod.Invoke(handler, new object[] { query, cancellationToken })!;
            var result = await resultTask;

            stopwatch.Stop();

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "Handled {RequestName} with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    requestName,
                    result.Error?.Code,
                    result.Error?.Description,
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error handling {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task<Result?> ValidateAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToList();
        if (!validators.Any())
        {
            return null;
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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

            return Result.Failure(error);
        }

        return null;
    }

    private async Task<Result<TResult>?> ValidateAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken)
    {
        var validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToList();
        if (!validators.Any())
        {
            return null;
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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

            return Result<TResult>.Failure(error);
        }

        return null;
    }

    private Task<Result<TResult>?> ValidateQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        var queryType = query.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(queryType);
        var validators = _serviceProvider.GetServices(validatorType).ToList();
        
        if (!validators.Any())
        {
            return Task.FromResult<Result<TResult>?>(null);
        }

        var failures = new List<FluentValidation.Results.ValidationFailure>();
        
        // Используем синхронную валидацию для queries (проще и быстрее)
        foreach (var validator in validators)
        {
            var validateMethod = validator.GetType().GetMethod("Validate", new[] { queryType });
            if (validateMethod != null)
            {
                var result = validateMethod.Invoke(validator, new[] { query });
                if (result is FluentValidation.Results.ValidationResult validationResult && !validationResult.IsValid)
                {
                    failures.AddRange(validationResult.Errors);
                }
            }
        }

        if (failures.Any())
        {
            var errorMessages = failures
                .Select(f => f.ErrorMessage)
                .ToList();

            var error = Error.Validation(
                "ValidationError",
                string.Join("; ", errorMessages));

            return Task.FromResult<Result<TResult>?>(Result<TResult>.Failure(error));
        }

        return Task.FromResult<Result<TResult>?>(null);
    }
}

