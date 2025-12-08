using Domain.Users.ValueObjects;

namespace Application.Users.Queries.GetUserById;

/// <summary>
/// User information response for GetUserById query
/// </summary>
public sealed record UserResponse(
    /// <summary>
    /// Unique identifier of the user
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid UserId,
    /// <summary>
    /// Tenant identifier the user belongs to
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid TenantId,
    /// <summary>
    /// Username
    /// </summary>
    /// <example>john_doe</example>
    string UserName,
    /// <summary>
    /// Email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    string Email,
    /// <summary>
    /// User role: Observer (0), Member (1), Admin (2), Owner (3)
    /// </summary>
    /// <example>1</example>
    UserRole Role) { }