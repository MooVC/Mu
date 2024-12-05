namespace Mu.Architecture.Messaging;

using System.Text.Json.Serialization;

public readonly record struct Context
{
    internal Context(Guid causation)
        : this(causation, causation)
    {
    }

    [JsonConstructor]
    internal Context(Guid causation, Guid correlation)
    {
        Causation = causation;
        Correlation = correlation;
    }

    public Guid Causation { get; }

    public Guid Correlation { get; }

    public bool IsInitiator => Causation == Correlation;
}