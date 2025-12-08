using Application.Abstractions.Data;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Behaviors;

internal sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        ApplicationDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "[TRANSACTION] Starting transaction for {RequestName}",
            requestName);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogDebug(
                    "[TRANSACTION] Executing {RequestName}",
                    requestName);

                var response = await next();

                _logger.LogDebug(
                    "[TRANSACTION] Committing transaction for {RequestName}",
                    requestName);

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "[TRANSACTION] Transaction committed for {RequestName}",
                    requestName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[TRANSACTION] Rolling back transaction for {RequestName}",
                    requestName);

                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });
    }
}

