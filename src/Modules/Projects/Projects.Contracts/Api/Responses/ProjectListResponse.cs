using Projects.Contracts.Enums;

namespace Projects.Contracts.Api.Responses;

/// <summary>
/// Ответ со списком проектов
/// </summary>
public sealed record ProjectListResponse(
    Guid Id,
    int TenantId,
    string Name,
    ProjectStatusContract Status,
    DateTime? UpdatedAt);

