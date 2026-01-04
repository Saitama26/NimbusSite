using Common.Domain.Results;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик команды (без возвращаемого значения)
/// Обрабатывает команду и изменяет состояние системы
/// </summary>
public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Обработчик команды с возвращаемым значением
/// Обрабатывает команду, изменяет состояние и возвращает результат
/// </summary>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken = default);
}