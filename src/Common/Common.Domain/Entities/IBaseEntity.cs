namespace Common.Domain.Entities;

/// <summary>
/// Базовый интерфейс для всех сущностей
/// </summary>
public interface IBaseEntity
{
    /// <summary>
    /// Уникальный идентификатор сущности
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Дата и время создания сущности
    /// </summary>
    DateTime CreatedAt { get; }
}

