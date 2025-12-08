namespace SharedKernel;

public record Result 
{
    public bool IsSuccess;
    public Error? Error;

    protected Result(bool isSuccess, Error? error) 
    {
        if (isSuccess && error != null ||
            !isSuccess && error == null) {
            throw new ArgumentException("Invalid operation");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error); 

    public static implicit operator Result(Error error) => Failure(error);
}

public record Result<T> : Result 
{
    public T? Value { get; }

    private Result(T value) : base(true, null) => Value = value;
    private Result(Error error) : base(false, error) { }

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}