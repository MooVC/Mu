namespace Mu.Communications.Messaging;

using System.Text.Json.Serialization;
using Mu.Communications.Tracing;

/// <summary>
/// Wraps a use case result as a synchronous outcome message.
/// </summary>
/// <typeparam name="TResult">The successful result type.</typeparam>
public sealed record Outcome<TResult>
    : Message
    where TResult : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> record.
    /// </summary>
    internal Outcome(Ledger ledger, Result<TResult> result)
        : this(ledger, DateTimeOffset.UtcNow, result)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Outcome{TResult}"/> record.
    /// </summary>
    [JsonConstructor]
    internal Outcome(Ledger ledger, DateTimeOffset preparedAt, Result<TResult> result)
        : base(ledger, preparedAt)
    {
        Result = result;
    }

    /// <summary>
    /// Gets the result carried by the outcome.
    /// </summary>
    public Result<TResult> Result { get; }

    /// <summary>
    /// Converts an outcome to its wrapped result.
    /// </summary>
    public static implicit operator Result<TResult>(Outcome<TResult> response)
    {
        return response.Result;
    }
}