namespace Mu.Modelling.Behavior;

using Mu.Modelling;

/// <summary>
/// Base abstraction for domain messages that capture cause and model context.
/// </summary>
public abstract record Causal
{
    private protected Causal()
        : this(DateTimeOffset.UtcNow)
    {
    }

    private protected Causal(Guid identity, DateTimeOffset proposed)
    {
        Identity = identity;
        Proposed = proposed;
    }

    private Causal(DateTimeOffset proposed)
        : this(Guid.CreateVersion7(proposed), proposed)
    {
    }

    /// <summary>
    /// Gets the causal identity for the message.
    /// </summary>
    public Guid Identity { get; }

    /// <summary>
    /// Gets the time at which the message was proposed.
    /// </summary>
    public DateTimeOffset Proposed { get; }

    /// <summary>
    /// Gets the aggregate model associated with the message.
    /// </summary>
    public abstract Model Model { get; }
}