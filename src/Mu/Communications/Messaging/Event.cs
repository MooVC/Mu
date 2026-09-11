namespace Mu.Communications.Messaging;

using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;
using ProtoBuf;

/// <summary>
/// Base type for asynchronous event messages that carry facts.
/// </summary>
[ProtoContract(SkipConstructor = true)]
public abstract record Event
    : Message
{
    private protected Event()
    {
    }

    /// <summary>
    /// Gets the time the fact was committed to persistence.
    /// </summary>
    [ProtoMember(1, Name = nameof(CommittedAt))]
    public abstract DateTimeOffset CommittedAt { get; }

    /// <summary>
    /// Gets the fact carried by the event.
    /// </summary>
    [ProtoIgnore]
    public abstract Fact Fact { get; }
}