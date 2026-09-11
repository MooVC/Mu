namespace Mu.Communications.Messaging;

using System;
using Mu.Communications.Tracing;
using ProtoBuf;

/// <summary>
/// Base type for messages exchanged across Mu communication boundaries.
/// </summary>
[ProtoContract(SkipConstructor = true)]
[ProtoInclude(100, typeof(Event))]
public abstract record Message
{
    private protected Message()
    {
    }

    /// <summary>
    /// Gets the tracing ledger for the message.
    /// </summary>
    [ProtoMember(1, Name = nameof(Ledger))]
    public abstract Ledger Ledger { get; }

    /// <summary>
    /// Gets the time the message was prepared.
    /// </summary>
    [ProtoMember(2, Name = nameof(PreparedAt))]
    public abstract DateTimeOffset PreparedAt { get; }
}