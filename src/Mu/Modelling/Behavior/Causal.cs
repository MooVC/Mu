namespace Mu.Modelling.Behavior;

using Mu.Modelling;
using ProtoBuf;

/// <summary>
/// Base abstraction for domain messages that capture cause and model context.
/// </summary>
[ProtoContract]
[ProtoInclude(100, typeof(Fact))]
[ProtoInclude(101, typeof(UseCase))]
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
    [ProtoMember(1, Name = nameof(Identity))]
    public Guid Identity { get; }

    /// <summary>
    /// Gets the time at which the message was proposed.
    /// </summary>
    [ProtoMember(2, Name = nameof(Proposed))]
    public DateTimeOffset Proposed { get; }

    /// <summary>
    /// Gets the aggregate model associated with the message.
    /// </summary>
    [ProtoIgnore]
    public abstract Representation Model { get; }
}