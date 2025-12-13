using Common.Domain.Results;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Базовый интерфейс команды (без возвращаемого значения)
/// Используется для команд CQRS, которые изменяют состояние системы
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Базовый интерфейс команды с возвращаемым значением
/// Используется для команд, которые возвращают результат (например, ID созданной сущности)
/// </summary>
public interface ICommand<TResult>
{
}
