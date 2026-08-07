namespace Mu.Communications.Messaging;

using System.Text.Json.Serialization;
using Mu.Communications.Tracing;
using ProtoBuf;

/// <summary>
/// Wraps a use case result as a synchronous outcome message.
/// </summary>
/// <typeparam name="TResult">The successful result type.</typeparam>
[ProtoContract(SkipConstructor = true)]
public sealed record Outcome<TResult>
    : Message
    where TResult : notnull
{
    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="Outcome{TResult}"/> record.
    /// </summary>
    internal Outcome(Ledger ledger, Result<TResult> result)
        : this(ledger, DateTimeOffset.UtcNow, result)
    {
    }

    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="Outcome{TResult}"/> record.
    /// </summary>
    [JsonConstructor]
    internal Outcome(Ledger ledger, DateTimeOffset preparedAt, Result<TResult> result)
        : base()
    {
        Ledger = ledger;
        PreparedAt = preparedAt;
        Result = result;
    }

    /// <summary>
    /// Gets the tracing ledger for the message.
    /// </summary>
    [ProtoMember(1, Name = nameof(Ledger))]
    public override Ledger Ledger { get; }

    /// <summary>
    /// Gets the time the message was prepared.
    /// </summary>
    [ProtoMember(2, Name = nameof(PreparedAt))]
    public override DateTimeOffset PreparedAt { get; }

    /// <summary>
    /// Gets the result carried by the outcome.
    /// </summary>
    [ProtoMember(3, Name = nameof(Result))]
    public Result<TResult> Result { get; }

    /// <summary>
    /// Converts an outcome to its wrapped result.
    /// </summary>
    public static implicit operator Result<TResult>(Outcome<TResult> response)
    {
        return response.Result;
    }
}