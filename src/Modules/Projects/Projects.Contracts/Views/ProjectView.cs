using Projects.Contracts.Enums;

namespace Projects.Contracts.Views;

/// <summary>
/// Database View модель проекта для других модулей (read-only)
/// Соответствует Database View vw_Projects
/// </summary>
public sealed class ProjectView
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatusContract Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

