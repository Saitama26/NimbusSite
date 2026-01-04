namespace Tasks.Application.Abstractions.Views;

/// <summary>
/// DTO проекта из view (Projects).
/// </summary>
public sealed class ProjectViewDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

