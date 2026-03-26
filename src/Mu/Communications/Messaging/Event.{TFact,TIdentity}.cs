namespace Mu.Communications.Messaging;

using System.Text.Json.Serialization;
using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;
using Mu.Modelling.State;

/// <summary>
/// Represents a concrete event containing a fact and its aggregate origin.
/// </summary>
/// <typeparam name="TFact">The emitted fact type.</typeparam>
/// <typeparam name="TIdentity">The aggregate identity type.</typeparam>
public sealed record Event<TFact, TIdentity>
    : Event
    where TFact : Fact
    where TIdentity : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Event{TFact, TIdentity}"/> record.
    /// </summary>
    [JsonConstructor]
    internal Event(DateTimeOffset committedAt, Ledger context, TFact fact, Reference<TIdentity> origin, DateTimeOffset preparedAt)
        : base(committedAt, context, fact, preparedAt)
    {
        Fact = fact;
        Origin = origin;
    }

    /// <summary>
    /// Gets the strongly-typed fact carried by the event.
    /// </summary>
    public new TFact Fact { get; }

    /// <summary>
    /// Gets the aggregate reference from which the fact originated.
    /// </summary>
    public Reference<TIdentity> Origin { get; }
}