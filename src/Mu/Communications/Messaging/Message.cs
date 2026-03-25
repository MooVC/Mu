namespace Mu.Communications.Messaging;

using System;
using Mu.Communications.Tracing;

/// <summary>
/// Base type for messages exchanged across Mu communication boundaries.
/// </summary>
public abstract record Message
{
    private protected Message(Ledger ledger)
        : this(ledger, DateTimeOffset.UtcNow)
    {
    }

    private protected Message(Ledger ledger, DateTimeOffset preparedAt)
    {
        Ledger = ledger;
        PreparedAt = preparedAt;
    }

    /// <summary>
    /// Gets the tracing ledger for the message.
    /// </summary>
    public Ledger Ledger { get; }

    /// <summary>
    /// Gets the time the message was prepared.
    /// </summary>
    public DateTimeOffset PreparedAt { get; }
}
