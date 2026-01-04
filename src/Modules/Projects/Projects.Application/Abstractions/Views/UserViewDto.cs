namespace Projects.Application.Abstractions.Views;

/// <summary>
/// DTO представление пользователя, читаемого через Database View из Users.
/// </summary>
public sealed class UserViewDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Status { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

