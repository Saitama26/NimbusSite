using Projects.Contracts.Enums;

namespace Projects.Contracts.Api.Responses;

/// <summary>
/// Ответ с информацией о проекте
/// </summary>
public sealed record ProjectResponse(
    Guid Id,
    int TenantId,
    string Name,
    string? Description,
    ProjectStatusContract Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

