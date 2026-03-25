namespace Mu.Modelling.Behavior;

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
