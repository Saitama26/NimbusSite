using System;

namespace Tasks.Infrastructure.Views.ProjectsViews;

/// <summary>
/// EF-проекция на vw_Projects (из базы Projects).
/// </summary>
public sealed class ProjectView
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

