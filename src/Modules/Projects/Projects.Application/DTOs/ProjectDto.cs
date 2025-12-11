namespace Projects.Application.DTOs;

/// <summary>
/// Полная информация о проекте
/// </summary>
public sealed class ProjectDto
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
    /// Описание проекта
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Статус проекта (Active, Archived, Deleted)
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Дата создания проекта
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Дата последнего обновления проекта
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

