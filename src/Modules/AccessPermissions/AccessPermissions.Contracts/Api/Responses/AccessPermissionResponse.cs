using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Api.Responses;

/// <summary>
/// Ответ с полной информацией о разрешении доступа (публичный API)
/// </summary>
public sealed record AccessPermissionResponse(
    Guid Id,
    int TenantId,
    Guid UserId,
    Guid? ProjectId,
    Guid? TaskId,
    PermissionScopeContract Scope,
    PermissionActionContract Action,
    PermissionTypeContract Type,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    string? Note,
    DateTime? UpdatedAt,
    bool IsValid);

