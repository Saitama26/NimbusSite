using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Api.Requests;

/// <summary>
/// Запрос на создание разрешения доступа (публичный API)
/// </summary>
public sealed record CreateAccessPermissionRequest(
    int TenantId,
    Guid UserId,
    PermissionScopeContract Scope,
    PermissionActionContract Action,
    PermissionTypeContract Type,
    Guid CreatedByUserId,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    DateTime? ExpiresAt = null,
    string? Note = null);

