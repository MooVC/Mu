namespace Mu.Architecture.Modelling;

public abstract record Query
    : NonMutational
{
    private protected Query()
    {
    }

    private protected Query(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected Query(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}