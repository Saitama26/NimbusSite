namespace Projects.Domain.Enums;

/// <summary>
/// Статус проекта.
/// </summary>
public enum ProjectStatus
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

