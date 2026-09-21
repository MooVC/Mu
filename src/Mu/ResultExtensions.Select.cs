namespace Mu;

/// <summary>
/// Provides projection helpers for asynchronous <see cref="Result{T}"/> values.
/// </summary>
public static partial class ResultExtensions
{
    extension<T>(Task<Result<T>> result)
        where T : notnull
    {
        /// <summary>
        /// Projects a successful result to a <see langword="new"/> value.
        /// </summary>
        public async Task<Result<TResult>> Select<TResult>(Func<T, TResult> success)
            where TResult : notnull
        {
            Result<T> value = await result.ConfigureAwait(false);

            return value.Select(success);
        }

        /// <summary>
        /// Projects a successful result to a <see langword="new"/> value using an asynchronous selector.
        /// </summary>
        public async Task<Result<TResult>> Select<TResult>(Func<T, Task<TResult>> success)
            where TResult : notnull
        {
            Result<T> value = await result.ConfigureAwait(false);

            return await value
                .Select(success)
                .ConfigureAwait(false);
        }
    }
}