using Application.Abstractions.Messaging;
using Domain.Users.ValueObjects;

namespace Application.Users.Commands.CreateUser;

/// <summary>
/// Command for creating a new user
/// </summary>
/// <param name="TenantId">Tenant identifier</param>
/// <param name="UserName">Unique username</param>
/// <param name="Email">User email address</param>
/// <param name="Password">User password (minimum 8 characters)</param>
/// <param name="Role">User role (Observer = 0, Member = 1, Admin = 2, Owner = 3)</param>
public sealed record CreateUserCommand(
    /// <summary>
    /// Tenant identifier
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    Guid TenantId,
    /// <summary>
    /// Unique username (max 100 characters)
    /// </summary>
    /// <example>john_doe</example>
    string UserName,
    /// <summary>
    /// User email address (must be valid email format)
    /// </summary>
    /// <example>john.doe@example.com</example>
    string Email,
    /// <summary>
    /// User password (minimum 8 characters)
    /// </summary>
    /// <example>SecurePassword123!</example>
    string Password,
    /// <summary>
    /// User role: Observer (0), Member (1), Admin (2), Owner (3)
    /// </summary>
    /// <example>1</example>
    UserRole Role
) : ICommand<Guid> {}