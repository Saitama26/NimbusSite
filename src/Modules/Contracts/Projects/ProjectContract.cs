namespace Contracts.Projects;

/// <summary>
/// Контракт для обмена данными о проекте между модулями
/// </summary>
public sealed record ProjectContract(
    Guid Id,
    Guid TenantId,
    string Name,
    ProjectStatusContract Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt = null,
    string? Description = null);

/// <summary>
/// Статус проекта
/// </summary>
public enum ProjectStatusContract
{
    Active = 1,
    Archived = 2,
    Deleted = 3
}

