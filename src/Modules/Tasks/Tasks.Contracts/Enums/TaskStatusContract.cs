namespace Tasks.Contracts.Enums;

/// <summary>
/// Статус задачи (публичный enum для API и Events)
/// </summary>
public enum TaskStatusContract
{
    /// <summary>
    /// Новая задача (только создана)
    /// </summary>
    New = 1,

    /// <summary>
    /// Задача в работе
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Задача на проверке
    /// </summary>
    InReview = 3,

    /// <summary>
    /// Задача выполнена
    /// </summary>
    Completed = 4,

    /// <summary>
    /// Задача отменена
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Задача заблокирована
    /// </summary>
    Blocked = 6,

    /// <summary>
    /// Задача удалена (soft delete)
    /// </summary>
    Deleted = 7
}

