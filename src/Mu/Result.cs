namespace Mu;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the outcome of an operation that can either succeed with a value or fail with validation errors.
/// </summary>
/// <typeparam name="T">The type of value produced when the operation succeeds.</typeparam>
public sealed record Result<T>
    where T : notnull
{
    private Result(T value)
        : this([], true, value)
    {
    }

    private Result(params IEnumerable<ValidationResult> failures)
        : this([.. failures], false, default)
    {
        if (Failures.Length == 0)
        {
            throw new ArgumentException("At least one failure must be provided for an unsuccessful result.", nameof(failures));
        }
    }

    [JsonConstructor]
    private Result(ImmutableArray<ValidationResult> failures, bool isSuccessful, T? value)
    {
        Failures = failures;
        IsSuccessful = isSuccessful;
        Value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the result is successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the validation failures associated with an unsuccessful result.
    /// </summary>
    public ImmutableArray<ValidationResult> Failures => IsSuccessful
        ? throw new InvalidOperationException("There are no failures associated with a successful result.")
        : field;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    public T? Value => IsSuccessful
        ? field
        : throw new InvalidOperationException("There is no value associated with an unsuccessful result.");

    /// <summary>
    /// Converts a successful value to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The successful value.</param>
    public static implicit operator Result<T>(T value)
    {
        return new(value);
    }

    /// <summary>
    /// Converts a single validation failure to an unsuccessful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="failure">The validation failure.</param>
    public static implicit operator Result<T>(ValidationResult failure)
    {
        return new(failure);
    }

    /// <summary>
    /// Converts multiple validation failures to an unsuccessful <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="failures">The validation failures.</param>
    public static implicit operator Result<T>(ImmutableArray<ValidationResult> failures)
    {
        return new(failures, false, default);
    }

    /// <summary>
    /// Executes callbacks depending on whether the result is successful.
    /// </summary>
    public Result<T> When(Action<ImmutableArray<ValidationResult>>? failure = default, Action<T>? success = default)
    {
        failure ??= _ => { };
        success ??= _ => { };

        if (IsSuccessful)
        {
            success(Value);
        }
        else
        {
            failure(Failures);
        }

        return this;
    }

    /// <summary>
    /// Executes asynchronous callbacks depending on whether the result is successful.
    /// </summary>
    public async Task<Result<T>> When(Func<ImmutableArray<ValidationResult>, Task>? failure = default, Func<T, Task>? success = default)
    {
        failure ??= _ => Task.CompletedTask;
        success ??= _ => Task.CompletedTask;

        Task action = IsSuccessful
            ? success(Value)
            : failure(Failures);

        await action.ConfigureAwait(false);

        return this;
    }

    /// <summary>
    /// Projects a successful value to a new result type.
    /// </summary>
    public Result<TResult> Select<TResult>(Func<T, TResult> success)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(success);

        if (IsSuccessful)
        {
            return success(Value);
        }

        return Failures;
    }

    /// <summary>
    /// Projects a successful value to a new result type asynchronously.
    /// </summary>
    public async Task<Result<TResult>> Select<TResult>(Func<T, Task<TResult>> success)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(success);

        if (IsSuccessful)
        {
            return await success(Value)
                .ConfigureAwait(false);
        }

        return Failures;
    }

    /// <summary>
    /// Projects the result to a value by handling both success and failure paths.
    /// </summary>
    public TResult Select<TResult>(Func<ImmutableArray<ValidationResult>, TResult> failure, Func<T, TResult> success)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccessful
            ? success(Value)
            : failure(Failures);
    }

    /// <summary>
    /// Projects the result to a value asynchronously by handling both success and failure paths.
    /// </summary>
    public Task<TResult> Select<TResult>(Func<ImmutableArray<ValidationResult>, Task<TResult>> failure, Func<T, Task<TResult>> success)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccessful
            ? success(Value)
            : failure(Failures);
    }
}