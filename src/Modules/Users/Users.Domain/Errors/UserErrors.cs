using Common.Domain.Results;

namespace Users.Domain.Errors;

/// <summary>
/// Ошибки домена Users
/// </summary>
public static class UserErrors
{
    /// <summary>
    /// Пользователь не найден
    /// </summary>
    public static Error NotFound(Guid userId) =>
        Error.NotFound(
            "User.NotFound",
            $"User with ID {userId} was not found.");

    /// <summary>
    /// Пользователь с указанным email уже существует в рамках тенанта
    /// </summary>
    public static Error EmailAlreadyExists(string email, Guid tenantId) =>
        Error.Conflict(
            "User.EmailAlreadyExists",
            $"User with email '{email}' already exists in tenant {tenantId}.");

    /// <summary>
    /// Email не может быть пустым
    /// </summary>
    public static Error EmailEmpty =>
        Error.Validation(
            "User.EmailEmpty",
            "User email cannot be empty.");

    /// <summary>
    /// Невалидный формат email
    /// </summary>
    public static Error InvalidEmailFormat =>
        Error.Validation(
            "User.InvalidEmailFormat",
            "Invalid email format.");

    /// <summary>
    /// Имя пользователя не может быть пустым
    /// </summary>
    public static Error NameEmpty =>
        Error.Validation(
            "User.NameEmpty",
            "User name cannot be empty.");

    /// <summary>
    /// Пользователь уже удален
    /// </summary>
    public static Error AlreadyDeleted =>
        Error.Conflict(
            "User.AlreadyDeleted",
            "User is already deleted.");

    /// <summary>
    /// Пользователь неактивен
    /// </summary>
    public static Error Inactive =>
        Error.Forbidden(
            "User.Inactive",
            "User is inactive and operations are not allowed.");

    /// <summary>
    /// Невозможно изменить статус пользователя
    /// </summary>
    public static Error CannotChangeStatus =>
        Error.Validation(
            "User.CannotChangeStatus",
            "Cannot change user status.");

    /// <summary>
    /// Невозможно изменить роль пользователя
    /// </summary>
    public static Error CannotChangeRole =>
        Error.Validation(
            "User.CannotChangeRole",
            "Cannot change user role.");

    /// <summary>
    /// Невалидный формат телефона
    /// </summary>
    public static Error InvalidPhoneFormat =>
        Error.Validation(
            "User.InvalidPhoneFormat",
            "Invalid phone number format.");
}

