using Application.Abstractions.Messaging;

namespace Application.Authentication.Commands.Login;

/// <summary>
/// Command for user authentication
/// </summary>
/// <param name="Email">User email address</param>
/// <param name="Password">User password</param>
public sealed record LoginCommand(
    /// <summary>
    /// User email address
    /// </summary>
    /// <example>user@example.com</example>
    string Email,
    /// <summary>
    /// User password
    /// </summary>
    /// <example>SecurePassword123!</example>
    string Password
) : ICommand<LoginResponse> { }

