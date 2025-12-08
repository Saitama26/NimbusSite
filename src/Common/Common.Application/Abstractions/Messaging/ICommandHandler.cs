using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик команды (без возвращаемого значения)
/// Обрабатывает команду и изменяет состояние системы
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Обработчик команды с возвращаемым значением
/// Обрабатывает команду, изменяет состояние и возвращает результат
/// </summary>
public interface ICommandHandler<TCommand, TResult> : IRequestHandler<TCommand, Result<TResult>>
    where TCommand : ICommand<TResult>
{
}