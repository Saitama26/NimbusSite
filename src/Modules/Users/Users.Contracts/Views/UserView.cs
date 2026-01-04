using Users.Contracts.Enums;

namespace Users.Contracts.Views;

/// <summary>
/// Database View модель пользователя для других модулей (read-only)
/// Соответствует Database View vw_Users
/// </summary>
public sealed class UserView
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserStatusContract Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
}

