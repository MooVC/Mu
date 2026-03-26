namespace Mu.Modelling.Behavior;

/// <summary>
/// Represents a use case that mutates aggregate state.
/// </summary>
public abstract record Mutational
    : UseCase
{
    private protected Mutational()
    {
    }

    private protected Mutational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}