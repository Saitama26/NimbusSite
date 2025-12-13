using Common.Domain.Results;

namespace Identity.Domain.Errors;

/// <summary>
/// Ошибки домена Identity
/// </summary>
public static class IdentityErrors
{
    public static Error UserNotFound(Guid userId) =>
        Error.NotFound("Identity.UserNotFound", $"User with ID {userId} was not found.");

    public static Error InvalidCredentials =>
        Error.Unauthorized("Identity.InvalidCredentials", "Invalid email or password.");

    public static Error EmailNotConfirmed =>
        Error.Forbidden("Identity.EmailNotConfirmed", "Email address is not confirmed.");

    public static Error UserLockedOut =>
        Error.Forbidden("Identity.UserLockedOut", "User account is locked out.");

    public static Error InvalidToken =>
        Error.Unauthorized("Identity.InvalidToken", "Invalid or expired token.");

    public static Error TokenExpired =>
        Error.Unauthorized("Identity.TokenExpired", "Token has expired.");

    public static Error TokenRevoked =>
        Error.Unauthorized("Identity.TokenRevoked", "Token has been revoked.");

    public static Error SessionNotFound(Guid sessionId) =>
        Error.NotFound("Identity.SessionNotFound", $"Session with ID {sessionId} was not found.");

    public static Error SessionExpired =>
        Error.Unauthorized("Identity.SessionExpired", "Session has expired.");

    public static Error SessionRevoked =>
        Error.Unauthorized("Identity.SessionRevoked", "Session has been revoked.");

    public static Error RefreshTokenNotFound =>
        Error.NotFound("Identity.RefreshTokenNotFound", "Refresh token was not found.");

    public static Error RefreshTokenExpired =>
        Error.Unauthorized("Identity.RefreshTokenExpired", "Refresh token has expired.");

    public static Error RefreshTokenRevoked =>
        Error.Unauthorized("Identity.RefreshTokenRevoked", "Refresh token has been revoked.");

    public static Error PasswordTooWeak =>
        Error.Validation("Identity.PasswordTooWeak", "Password does not meet security requirements.");

    public static Error PasswordMismatch =>
        Error.Validation("Identity.PasswordMismatch", "Current password is incorrect.");

    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("Identity.EmailAlreadyExists", $"Email {email} is already registered.");

    public static Error InvalidEmailFormat =>
        Error.Validation("Identity.InvalidEmailFormat", "Invalid email format.");

    public static Error PasswordResetTokenInvalid =>
        Error.Unauthorized("Identity.PasswordResetTokenInvalid", "Password reset token is invalid or expired.");

    public static Error EmailConfirmationTokenInvalid =>
        Error.Unauthorized("Identity.EmailConfirmationTokenInvalid", "Email confirmation token is invalid or expired.");
}

