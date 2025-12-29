namespace Tasks.Contracts.Enums;

/// <summary>
/// Приоритет задачи (публичный enum для API и Events)
/// </summary>
public enum TaskPriorityContract
{
    /// <summary>
    /// Низкий приоритет
    /// </summary>
    Low = 1,

    /// <summary>
    /// Обычный приоритет
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Высокий приоритет
    /// </summary>
    High = 3,

    /// <summary>
    /// Критический приоритет
    /// </summary>
    Critical = 4
}

