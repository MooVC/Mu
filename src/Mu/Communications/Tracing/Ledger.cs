namespace Mu.Communications.Tracing;

using System.Text.Json.Serialization;
using ProtoBuf;

/// <summary>
/// Tracks causation and correlation identifiers across message flow.
/// </summary>
[ProtoContract]
public readonly record struct Ledger
{
    /// <summary>
    /// Initializes a <see langword="new"/> root ledger where causation and correlation are the same.
    /// </summary>
    internal Ledger(Guid causation)
        : this(causation, causation)
    {
    }

    /// <summary>
    /// Initializes a <see langword="new"/> ledger with explicit causation and correlation identifiers.
    /// </summary>
    [JsonConstructor]
    internal Ledger(Guid causation, Guid correlation)
    {
        Causation = causation;
        Correlation = correlation;
    }

    /// <summary>
    /// Gets the causation identifier for the current message.
    /// </summary>
    [ProtoMember(1, Name = nameof(Causation))]
    public Guid Causation { get; }

    /// <summary>
    /// Gets the correlation identifier for the message chain.
    /// </summary>
    [ProtoMember(2, Name = nameof(Correlation))]
    public Guid Correlation { get; }

    /// <summary>
    /// Gets a value indicating whether the current message started the correlation chain.
    /// </summary>
    [ProtoIgnore]
    public bool IsInitiator => Causation == Correlation;

    /// <summary>
    /// Creates the next ledger in the chain with a <see langword="new"/> causation identifier.
    /// </summary>
    public Ledger Next(Guid causation)
    {
        return new Ledger(causation, Correlation);
    }
}