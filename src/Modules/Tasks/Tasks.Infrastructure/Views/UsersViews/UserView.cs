using System;

namespace Tasks.Infrastructure.Views.UsersViews;

/// <summary>
/// EF-проекция на vw_Users (из базы Users).
/// </summary>
public sealed class UserView
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

