namespace Mu;

/// <summary>
/// Provides asynchronous continuation helpers for <see cref="Result{T}"/>.
/// </summary>
public static partial class ResultExtensions
{
    extension<T>(Task<Result<T>> result)
        where T : notnull
    {
        /// <summary>
        /// Executes an asynchronous action when the result is successful.
        /// </summary>
        public async Task<Result<T>> Then(Func<T, Task> success)
        {
            Result<T> value = await result.ConfigureAwait(false);

            return await value
                .When(success: success)
                .ConfigureAwait(false);
        }
    }
}