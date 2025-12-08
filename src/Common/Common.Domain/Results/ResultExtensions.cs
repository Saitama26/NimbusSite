namespace Common.Domain.Results;

/// <summary>
/// Расширения для Result
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Преобразовать Result&lt;T&gt; в Result&lt;U&gt; с помощью функции
    /// </summary>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
    {
        if (!result.IsSuccess)
        {
            return Result<U>.Failure(result.Error!);
        }

        return Result<U>.Success(mapper(result.Value!));
    }

    /// <summary>
    /// Преобразовать Result&lt;T&gt; в Result&lt;U&gt; с помощью функции, возвращающей Result&lt;U&gt;
    /// </summary>
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> binder)
    {
        if (!result.IsSuccess)
        {
            return Result<U>.Failure(result.Error!);
        }

        return binder(result.Value!);
    }

    /// <summary>
    /// Выполнить действие, если результат успешен
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    /// <summary>
    /// Выполнить действие, если результат неуспешен
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (!result.IsSuccess)
        {
            action(result.Error!);
        }

        return result;
    }

    /// <summary>
    /// Получить значение или значение по умолчанию
    /// </summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? defaultValue = default)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }
}

