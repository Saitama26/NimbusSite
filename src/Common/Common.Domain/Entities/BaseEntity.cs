namespace Common.Domain.Entities;

/// <summary>
/// Базовая реализация сущности
/// Простая сущность без доменной логики
/// </summary>
public abstract class BaseEntity : IBaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }
}

