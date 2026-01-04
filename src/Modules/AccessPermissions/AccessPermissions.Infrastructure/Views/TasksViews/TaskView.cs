using System;

namespace AccessPermissions.Infrastructure.Views.TasksViews;

public sealed class TaskView
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

