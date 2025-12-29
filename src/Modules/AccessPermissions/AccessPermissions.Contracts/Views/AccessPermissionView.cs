using AccessPermissions.Contracts.Enums;

namespace AccessPermissions.Contracts.Views;

/// <summary>
/// View модель разрешения доступа для других модулей (read-only)
/// Соответствует Database View vw_AccessPermissions
/// </summary>
public sealed class AccessPermissionView
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public PermissionScopeContract Scope { get; set; }
    public PermissionActionContract Action { get; set; }
    public PermissionTypeContract Type { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Note { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsValid { get; set; }
}

