using System;

namespace AccessPermissions.Application.Abstractions.Views;

public sealed class TaskViewDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid TenantId { get; init; }
    public string Title { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

