namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a mutational use case that transitions existing aggregate state.
/// </summary>
public abstract record Transitional
    : Mutational
{
    private protected Transitional()
    {
    }

    private protected Transitional(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}