using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Api.Requests;

/// <summary>
/// Запрос на обновление разрешения доступа (публичный API)
/// </summary>
public sealed record UpdateAccessPermissionRequest(
    PermissionTypeContract? Type = null,
    DateTime? ExpiresAt = null,
    string? Note = null);

