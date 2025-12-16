namespace Common.Application.Abstractions.Messaging;

/// <summary>
/// Базовый интерфейс запроса
/// Используется для запросов CQRS, которые читают данные без изменения состояния
/// </summary>
public interface IQuery<TResult>
{
}
