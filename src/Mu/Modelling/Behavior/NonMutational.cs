namespace Mu.Modelling.Behavior;

using ProtoBuf;

/// <summary>
/// Represents a use case that observes state without mutation.
/// </summary>
public abstract record NonMutational
    : UseCase
{
    private protected NonMutational()
    {
    }

    private protected NonMutational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}