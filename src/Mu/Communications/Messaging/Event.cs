namespace Mu.Communications.Messaging;

using Mu.Communications.Tracing;
using Mu.Modelling.Behavior;

/// <summary>
/// Base type for asynchronous event messages that carry facts.
/// </summary>
public abstract record Event
    : Message
{
    private protected Event(DateTimeOffset committedAt, Ledger context, Fact fact, DateTimeOffset preparedAt)
        : base(context, preparedAt)
    {
        ArgumentNullException.ThrowIfNull(fact);

        CommittedAt = committedAt;
        Fact = fact;
    }

    /// <summary>
    /// Gets the time the fact was committed to persistence.
    /// </summary>
    public DateTimeOffset CommittedAt { get; }

    /// <summary>
    /// Gets the fact carried by the event.
    /// </summary>
    public Fact Fact { get; }
}
