using Domain.Users.ValueObjects;

namespace Application.Users.Queries.GetUsers;

/// <summary>
/// User information response
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
    UserRole Role,
    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    /// <example>true</example>
    bool IsActive,
    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    DateTime CreatedAt) { }

