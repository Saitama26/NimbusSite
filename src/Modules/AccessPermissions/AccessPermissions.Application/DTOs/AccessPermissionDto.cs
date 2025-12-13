using AccessPermissions.Domain.Enums;

namespace AccessPermissions.Application.DTOs;

/// <summary>
/// DTO для разрешения доступа
/// </summary>
public sealed record AccessPermissionDto(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    Guid? ProjectId,
    Guid? TaskId,
    PermissionScope Scope,
    PermissionAction Action,
    PermissionType Type,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? ExpiresAt = null,
    string? Note = null,
    DateTime? UpdatedAt = null,
    bool IsValid = true);

