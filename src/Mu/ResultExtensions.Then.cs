namespace Mu;

/// <summary>
/// Provides asynchronous continuation helpers for <see cref="Result{T}"/>.
/// </summary>
public static partial class ResultExtensions
{
    /// <summary>
    /// Executes an asynchronous action when the result is successful.
    /// </summary>
    public static async Task<Result<T>> Then<T>(this Task<Result<T>> result, Func<T, Task> success)
        where T : notnull
    {
        Result<T> value = await result.ConfigureAwait(false);

        return await value
            .When(success: success)
            .ConfigureAwait(false);
    }
}
