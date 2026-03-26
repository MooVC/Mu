namespace Mu.Communications.Tracing;

using System.Text.Json.Serialization;

/// <summary>
/// Tracks causation and correlation identifiers across message flow.
/// </summary>
public readonly record struct Ledger
{
    /// <summary>
    /// Initializes a new root ledger where causation and correlation are the same.
    /// </summary>
    internal Ledger(Guid causation)
        : this(causation, causation)
    {
    }

    /// <summary>
    /// Initializes a new ledger with explicit causation and correlation identifiers.
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
    public Guid Causation { get; }

    /// <summary>
    /// Gets the correlation identifier for the message chain.
    /// </summary>
    public Guid Correlation { get; }

    /// <summary>
    /// Gets a value indicating whether the current message started the correlation chain.
    /// </summary>
    public bool IsInitiator => Causation == Correlation;

    /// <summary>
    /// Creates the next ledger in the chain with a new causation identifier.
    /// </summary>
    public Ledger Next(Guid causation)
    {
        return new Ledger(causation, Correlation);
    }
}