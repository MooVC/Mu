namespace Mu.Communications.Messaging;

using System.Text.Json.Serialization;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;
using ProtoBuf;

/// <summary>
/// Represents a concrete event containing a fact and its aggregate origin.
/// </summary>
/// <typeparam name="TFact">The emitted fact type.</typeparam>
/// <typeparam name="TIdentity">The aggregate identity type.</typeparam>
[ProtoContract(SkipConstructor = true)]
public sealed record Event<TFact, TIdentity>
    : Event
    where TFact : Fact
    where TIdentity : struct
{
    /// <summary>
    /// Initializes a <see langword="new"/> instance of the <see cref="Event{TFact, TIdentity}"/> record.
    /// </summary>
    [JsonConstructor]
    internal Event(DateTimeOffset committedAt, Ledger context, TFact fact, Reference<TIdentity> origin, DateTimeOffset preparedAt)
        : base()
    {
        ArgumentNullException.ThrowIfNull(fact);

        CommittedAt = committedAt;
        Fact = fact;
        Ledger = context;
        Origin = origin;
        PreparedAt = preparedAt;
    }

    /// <summary>
    /// Gets the time the fact was committed to persistence.
    /// </summary>
    [ProtoMember(3, Name = nameof(CommittedAt))]
    public override DateTimeOffset CommittedAt { get; }

    /// <summary>
    /// Gets the strongly-typed fact carried by the event.
    /// </summary>
    [ProtoMember(4, Name = nameof(Fact))]
    public override TFact Fact { get; }

    /// <summary>
    /// Gets the tracing ledger for the message.
    /// </summary>
    [ProtoMember(1, Name = nameof(Ledger))]
    public override Ledger Ledger { get; }

    /// <summary>
    /// Gets the aggregate reference from which the fact originated.
    /// </summary>
    [ProtoMember(5, Name = nameof(Origin))]
    public Reference<TIdentity> Origin { get; }

    /// <summary>
    /// Gets the time the message was prepared.
    /// </summary>
    [ProtoMember(2, Name = nameof(PreparedAt))]
    public override DateTimeOffset PreparedAt { get; }
}