namespace Common.Domain.Results;

/// <summary>
/// Тип ошибки
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Ошибка валидации
    /// </summary>
    Validation = 400,

    /// <summary>
    /// Не авторизован
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Доступ запрещен
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// Ресурс не найден
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// Конфликт (например, дубликат)
    /// </summary>
    Conflict = 409,

    /// <summary>
    /// Общая ошибка выполнения
    /// </summary>
    Failure = 500
}

/// <summary>
/// Класс ошибки
/// </summary>
public sealed class Error
{
    /// <summary>
    /// Тип ошибки
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Код ошибки (для клиента)
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Человекочитаемое описание ошибки
    /// </summary>
    public string Description { get; }

    internal Error(ErrorType type, string code, string description)
    {
        Type = type;
        Code = code;
        Description = description;
    }

    /// <summary>
    /// Создать ошибку валидации
    /// </summary>
    public static Error Validation(string code, string description) =>
        new(ErrorType.Validation, code, description);

    /// <summary>
    /// Создать ошибку "не авторизован"
    /// </summary>
    public static Error Unauthorized(string code, string description) =>
        new(ErrorType.Unauthorized, code, description);

    /// <summary>
    /// Создать ошибку "доступ запрещен"
    /// </summary>
    public static Error Forbidden(string code, string description) =>
        new(ErrorType.Forbidden, code, description);

    /// <summary>
    /// Создать ошибку "не найдено"
    /// </summary>
    public static Error NotFound(string code, string description) =>
        new(ErrorType.NotFound, code, description);

    /// <summary>
    /// Создать ошибку конфликта
    /// </summary>
    public static Error Conflict(string code, string description) =>
        new(ErrorType.Conflict, code, description);

    /// <summary>
    /// Создать общую ошибку
    /// </summary>
    public static Error Failure(string code, string description) =>
        new(ErrorType.Failure, code, description);

    /// <summary>
    /// Создать общую ошибку с сообщением исключения
    /// </summary>
    public static Error Failure(string code, Exception exception) =>
        new(ErrorType.Failure, code, exception.Message);

    public override string ToString() => $"{Code}: {Description}";
}

