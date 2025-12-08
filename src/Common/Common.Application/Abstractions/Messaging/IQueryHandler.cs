using Common.Domain.Results;
using MediatR;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик запроса
/// Обрабатывает запрос и возвращает данные без изменения состояния системы
/// </summary>
public interface IQueryHandler<TQuery, TResult> : IRequestHandler<TQuery, Result<TResult>>
    where TQuery : IQuery<TResult>
{
}

