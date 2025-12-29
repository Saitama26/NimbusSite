using System;

namespace Identity.Infrastructure.Views.UsersViews;

public sealed class UserView
{
    public Guid Id { get; set; }
    public int TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

