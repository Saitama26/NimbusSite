using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using MediatR;
using ISender = Common.Application.Abstractions.Messaging.ISender;

namespace Common.Infrastructure.Messaging;

/// <summary>
/// Реализация ISender через MediatR
/// </summary>
public sealed class MediatRSender : ISender
{
    private readonly IMediator _mediator;

    public MediatRSender(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result> Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result as Result ?? Result.Failure(Error.Failure("InvalidResult", "Command handler returned invalid result"));
    }

    public async Task<Result<TResult>> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result as Result<TResult> ?? Result<TResult>.Failure(Error.Failure("InvalidResult", "Command handler returned invalid result"));
    }

    public async Task<Result<TResult>> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return result as Result<TResult> ?? Result<TResult>.Failure(Error.Failure("InvalidResult", "Query handler returned invalid result"));
    }
}
