namespace Projects.Application.DTOs;

/// <summary>
/// Краткая информация о проекте для списка
/// </summary>
public sealed class ProjectListItemDto
{
    /// <summary>
    /// Идентификатор проекта
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор тенанта
    /// </summary>
    public Guid TenantId { get; set; }
    
    /// <summary>
    /// Название проекта
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Статус проекта (Active, Archived, Deleted)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата последнего обновления проекта
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

