using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Common.Infrastructure.Behaviors;

/// <summary>
/// Behavior для логирования выполнения команд и запросов
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName}",
            requestName);

        try
        {
            var response = await next();

            stopwatch.Stop();

            if (response is Result result && result.IsSuccess)
            {
                _logger.LogInformation(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
            else if (response is Result failedResult && !failedResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Handled {RequestName} with error {ErrorCode}: {ErrorDescription} in {ElapsedMilliseconds}ms",
                    requestName,
                    failedResult.Error?.Code,
                    failedResult.Error?.Description,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
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
}
