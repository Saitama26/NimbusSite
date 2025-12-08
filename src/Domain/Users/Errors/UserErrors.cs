using SharedKernel;

namespace Domain.Users.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid userId) =>
        new Error("User.NotFound",
            $"The user with the id - {userId} was not found",
            ErrorType.NotFound);

    public static Error NotFoundByEmail(string email) =>
        new Error("User.NotFoundByEmail",
            $"The user with the email '{email}' was not found",
            ErrorType.NotFound);

    public static Error AlreadyExists(string email) =>
        new Error("User.AlreadyExists",
            $"The user with the email '{email}' already exists",
            ErrorType.Conflict);

    public static Error Unauthorized() =>
        new Error("User.Unauthorized",
            "You are not authorized to perform this action.",
            ErrorType.Unauthorized);

    public static Error InvalidCredentials() =>
        new Error("User.InvalidCredentials",
            "The email or password is incorrect.",
            ErrorType.Validation);

    public static Error InvalidEmail() =>
        new Error("User.InvalidEmail",
            "The email format is invalid.",
            ErrorType.Validation);

    public static Error InvalidRefreshToken() =>
        new Error("User.InvalidRefreshToken",
            "The refresh token is invalid or expired.",
            ErrorType.Unauthorized);

    public static Error RefreshTokenExpired() =>
        new Error("User.RefreshTokenExpired",
            "The refresh token has expired.",
            ErrorType.Unauthorized);
}