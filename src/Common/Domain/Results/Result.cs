using System.Diagnostics.CodeAnalysis;

namespace Rtl.Core.Domain.Results;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error if the operation failed.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Pattern matching for Result. Executes onSuccess if successful, onFailure otherwise.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// The value of a successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of a failed result.</exception>
    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Creates a validation failure result.
    /// </summary>
    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Pattern matching for Result. Executes onSuccess if successful, onFailure otherwise.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error);

    /// <summary>
    /// Pattern matching for Result. Executes onSuccess if successful, onFailure otherwise.
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure) =>
        IsSuccess ? await onSuccess(_value!) : await onFailure(Error);
}
