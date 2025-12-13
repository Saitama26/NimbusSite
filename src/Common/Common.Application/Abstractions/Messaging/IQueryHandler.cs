using Common.Domain.Results;

namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Обработчик запроса
/// Обрабатывает запрос и возвращает данные без изменения состояния системы
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken = default);
}

