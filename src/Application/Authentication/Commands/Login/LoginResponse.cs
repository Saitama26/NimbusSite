namespace Application.Authentication.Commands.Login;

/// <summary>
/// Response containing authentication tokens and user information
/// </summary>
/// <param name="UserId">Unique identifier of the authenticated user</param>
/// <param name="UserName">Username of the authenticated user</param>
/// <param name="Email">Email address of the authenticated user</param>
/// <param name="AccessToken">JWT access token for API authentication</param>
/// <param name="RefreshToken">Refresh token for obtaining new access tokens</param>
/// <param name="ExpiresAt">Expiration date and time of the refresh token</param>
public sealed record LoginResponse(
    /// <summary>
    /// Unique identifier of the authenticated user
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid UserId,
    /// <summary>
    /// Username of the authenticated user
    /// </summary>
    /// <example>john_doe</example>
    string UserName,
    /// <summary>
    /// Email address of the authenticated user
    /// </summary>
    /// <example>user@example.com</example>
    string Email,
    /// <summary>
    /// JWT access token for API authentication
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c</example>
    string AccessToken,
    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    string RefreshToken,
    /// <summary>
    /// Expiration date and time of the refresh token
    /// </summary>
    /// <example>2024-12-31T23:59:59Z</example>
    DateTime ExpiresAt) { }

