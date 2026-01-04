using System;

namespace Identity.Application.Abstractions.Views;

using Users.Contracts.Enums;

public sealed class UserViewDto
{
    public Guid Id { get; init; }
    public int TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public UserStatusContract Status { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

