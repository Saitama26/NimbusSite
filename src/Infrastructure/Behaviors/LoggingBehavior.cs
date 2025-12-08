using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infrastructure.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
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
        var requestGuid = Guid.NewGuid();

        _logger.LogInformation(
            "[START] {RequestName} [{RequestGuid}]",
            requestName,
            requestGuid);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            if (response is SharedKernel.Result result && !result.IsSuccess)
            {
                _logger.LogWarning(
                    "[FAILURE] {RequestName} [{RequestGuid}] - {Error} - Duration: {Duration}ms",
                    requestName,
                    requestGuid,
                    result.Error?.Description ?? "Unknown error",
                    stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "[SUCCESS] {RequestName} [{RequestGuid}] - Duration: {Duration}ms",
                    requestName,
                    requestGuid,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[ERROR] {RequestName} [{RequestGuid}] - {Error} - Duration: {Duration}ms",
                requestName,
                requestGuid,
                ex.Message,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}

