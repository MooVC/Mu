namespace Mu;

using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public record Result<T>
    where T : notnull
{
    private Result(T value)
        : this([], true, value)
    {
    }

    private Result(params ReadOnlySpan<ValidationResult> failures)
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

    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccessful { get; }

    public ImmutableArray<ValidationResult> Failures => IsSuccessful
        ? throw new InvalidOperationException("There are no failures associated with a successful result.")
        : field;

    public T? Value => IsSuccessful
        ? field
        : throw new InvalidOperationException("There is no value associated with an unsuccessful result.");

    public static implicit operator Result<T>(T value)
    {
        return new(value);
    }

    public static implicit operator Result<T>(ValidationResult failure)
    {
        return new(failure);
    }

    public static implicit operator Result<T>(ValidationResult[] failures)
    {
        return new(failures);
    }

    public Task Switch(Func<ImmutableArray<ValidationResult>, Task> failure, Func<T, Task> success)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccessful
            ? success(Value)
            : failure(Failures);
    }

    public Task<TResult> Match<TResult>(Func<ImmutableArray<ValidationResult>, Task<TResult>> failure, Func<T, Task<TResult>> success)
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);

        return IsSuccessful
            ? success(Value)
            : failure(Failures);
    }
}