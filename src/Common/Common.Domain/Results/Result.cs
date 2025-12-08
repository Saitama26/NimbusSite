namespace Common.Domain.Results;

/// <summary>
/// Результат выполнения операции (Success или Failure)
/// </summary>
public class Result
{
    /// <summary>
    /// Успешно ли выполнена операция
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Ошибка (если IsSuccess == false)
    /// </summary>
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Успешный результат
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Неуспешный результат с ошибкой
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Неуспешный результат с ошибкой
    /// </summary>
    public static Result Failure(ErrorType type, string code, string description) =>
        new(false, new Error(type, code, description));

    /// <summary>
    /// Неявное преобразование Error в Result
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Результат выполнения операции со значением (Success или Failure)
/// </summary>
public class Result<T> : Result
{
    /// <summary>
    /// Значение результата (если IsSuccess == true)
    /// </summary>
    public T? Value { get; }

    private Result(bool isSuccess, T? value, Error? error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Успешный результат со значением
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Неуспешный результат с ошибкой
    /// </summary>
    public new static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>
    /// Неуспешный результат с ошибкой
    /// </summary>
    public new static Result<T> Failure(ErrorType type, string code, string description) =>
        new(false, default, new Error(type, code, description));

    /// <summary>
    /// Неявное преобразование T в Result&lt;T&gt;
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Неявное преобразование Error в Result&lt;T&gt;
    /// </summary>
    public static implicit operator Result<T>(Error error) => Failure(error);
}

