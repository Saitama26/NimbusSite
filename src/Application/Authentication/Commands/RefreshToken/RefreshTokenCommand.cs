using Application.Abstractions.Messaging;

namespace Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Command for refreshing access token
/// </summary>
/// <param name="RefreshToken">Valid refresh token obtained during login</param>
public sealed record RefreshTokenCommand(
    /// <summary>
    /// Valid refresh token obtained during login
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string RefreshToken
) : ICommand<RefreshTokenResponse> { }

