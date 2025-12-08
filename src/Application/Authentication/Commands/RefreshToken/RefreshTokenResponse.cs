namespace Application.Authentication.Commands.RefreshToken;

/// <summary>
/// Response containing new authentication tokens
/// </summary>
/// <param name="AccessToken">New JWT access token</param>
/// <param name="RefreshToken">New refresh token</param>
/// <param name="ExpiresAt">Expiration date and time of the refresh token</param>
public sealed record RefreshTokenResponse(
    /// <summary>
    /// New JWT access token
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string AccessToken,
    /// <summary>
    /// New refresh token
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string RefreshToken,
    /// <summary>
    /// Expiration date and time of the refresh token
    /// </summary>
    /// <example>2024-12-31T23:59:59Z</example>
    DateTime ExpiresAt) { }

