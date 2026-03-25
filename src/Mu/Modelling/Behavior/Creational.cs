namespace Mu.Modelling.Behavior;

/// <summary>
/// Represents a mutational use case that creates a new aggregate instance.
/// </summary>
public abstract record Creational
    : Mutational
{
    private protected Creational()
    {
    }

    private protected Creational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}
