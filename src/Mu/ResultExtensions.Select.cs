namespace Mu;

/// <summary>
/// Provides projection helpers for asynchronous <see cref="Result{T}"/> values.
/// </summary>
public static partial class ResultExtensions
{
    /// <summary>
    /// Projects a successful result to a new value.
    /// </summary>
    public static async Task<Result<TResult>> Select<T, TResult>(this Task<Result<T>> result, Func<T, TResult> success)
        where T : notnull
        where TResult : notnull
    {
        Result<T> value = await result.ConfigureAwait(false);

        return value.Select(success);
    }

    /// <summary>
    /// Projects a successful result to a new value using an asynchronous selector.
    /// </summary>
    public static async Task<Result<TResult>> Select<T, TResult>(this Task<Result<T>> result, Func<T, Task<TResult>> success)
        where T : notnull
        where TResult : notnull
    {
        Result<T> value = await result.ConfigureAwait(false);

        return await value
            .Select(success)
            .ConfigureAwait(false);
    }
}