using Application.Abstractions.Messaging;

namespace Application.Authentication.Commands.Logout;

/// <summary>
/// Command for user logout
/// </summary>
/// <param name="RefreshToken">Refresh token to invalidate</param>
public sealed record LogoutCommand(
    /// <summary>
    /// Refresh token to invalidate
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string RefreshToken
) : ICommand { }

