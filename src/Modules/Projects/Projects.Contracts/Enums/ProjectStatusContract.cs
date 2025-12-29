namespace Projects.Contracts.Enums;

/// <summary>
/// Статус проекта (публичный enum для API и Events)
/// </summary>
public enum ProjectStatusContract
{
    /// <summary>
    /// Активный проект
    /// </summary>
    Active = 1,

    /// <summary>
    /// Архивный проект
    /// </summary>
    Archived = 2,

    /// <summary>
    /// Удаленный проект (soft delete)
    /// </summary>
    Deleted = 3
}

