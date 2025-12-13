using Common.Domain.Results;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Интерфейс для отправки команд и запросов
/// </summary>
public interface ISender
{
    /// <summary>
    /// Отправить команду без возвращаемого значения
    /// </summary>
    Task<Result> Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    /// Отправить команду с возвращаемым значением
    /// </summary>
    Task<Result<TResult>> Send<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Отправить запрос
    /// </summary>
    Task<Result<TResult>> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}

