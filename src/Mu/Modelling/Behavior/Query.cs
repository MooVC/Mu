namespace Mu.Modelling.Behavior;

/// <summary>
/// Represents a non-mutational query use case.
/// </summary>
public abstract record Query
    : NonMutational
{
    private protected Query()
    {
    }

    private protected Query(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}