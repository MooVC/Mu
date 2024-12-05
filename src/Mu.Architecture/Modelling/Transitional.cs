namespace Mu.Architecture.Modelling;

public abstract record Transitional
    : Mutational
{
    private protected Transitional()
    {
    }

    private protected Transitional(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected Transitional(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}