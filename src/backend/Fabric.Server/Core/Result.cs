
namespace Fabric.Server.Core;

public readonly struct Unit
{
    public static readonly Unit Value = new();
}


public readonly struct Result<TValue, TError>
{
    private readonly bool _isSuccess;
    private readonly TError _error;
    private readonly TValue _value;

    private Result(bool isSuccess, TError error, TValue value)
    {
        _isSuccess = isSuccess;
        _error = error;
        _value = value;
    }

    public bool IsSuccess(out TValue value)
    {
        if (_isSuccess)
        {
            value = _value;
            return true;
        }

        value = default!;
        return false;
    }

    public bool IsFailure(out TError error)
    {
        if (!_isSuccess)
        {
            error = _error;
            return true;
        }

        error = default!;
        return false;
    }

    // Factories
    public static Result<TValue, TError> Success(TValue value) => new(true, default!, value);

    public static Result<TValue, TError> Failure(TError error) => new(false, error, default!);

    // Match
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure) =>
        _isSuccess ? onSuccess(_value) : onFailure(_error);

    public Result<TNew, TError> Map<TNew>(Func<TValue, TNew> func) => Result.Map(this, func);
}

public readonly struct Result<TError>
{
    private readonly Result<Unit, TError> _inner;

    private Result(Result<Unit, TError> inner)
    {
        _inner = inner;
    }

    public bool IsSuccess(out Unit value) => _inner.IsSuccess(out value);

    public bool IsFailure(out TError error) => _inner.IsFailure(out error);

    public static Result<TError> Success() => new(Result<Unit, TError>.Success(Unit.Value));

    public static Result<TError> Failure(TError error) => new(Result<Unit, TError>.Failure(error));

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<TError, TResult> onFailure) => _inner.Match(_ => onSuccess(), onFailure);
}

public static class Result
{
    public static Result<TError> Failure<TError>(TError error) => Result<TError>.Failure(error);

    public static Result<TValue, TError> Failure<TValue, TError>(TError error) => Result<TValue, TError>.Failure(error);

    public static Result<TError> Success<TError>() => Result<TError>.Success();

    public static Result<TValue, TError> Success<TValue, TError>(TValue value) => Result<TValue, TError>.Success(value);

    public static Result<TNew, TError>
        Map<TValue, TError, TNew>(Result<TValue, TError> result, Func<TValue, TNew> func)
    {
        return result.Match(
            v => Success<TNew, TError>(func(v)),
            Failure<TNew, TError>
        );
    }
}
