namespace DocManager.Application.Common;

public class Result
{
    protected Result(ResultStatus status, IReadOnlyList<string> errors)
    {
        Status = status;
        Errors = errors;
    }

    public ResultStatus Status { get; }

    public IReadOnlyList<string> Errors { get; }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success, []);

    public static Result NotFound(string message) => new(ResultStatus.NotFound, [message]);

    public static Result ValidationError(IEnumerable<string> errors) =>
        new(ResultStatus.ValidationError, errors.ToList());
}

public sealed class Result<T> : Result
{
    private Result(ResultStatus status, T? value, IReadOnlyList<string> errors)
        : base(status, errors)
    {
        Value = value;
    }

    /// <summary>The payload; only set when <see cref="Result.IsSuccess"/> is true.</summary>
    public T? Value { get; }

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, []);

    public static new Result<T> NotFound(string message) => new(ResultStatus.NotFound, default, [message]);

    public static new Result<T> ValidationError(IEnumerable<string> errors) =>
        new(ResultStatus.ValidationError, default, errors.ToList());
}
