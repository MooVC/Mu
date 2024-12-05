namespace Mu.Architecture.Modelling;

public abstract record NonMutational
    : UseCase
{
    private protected NonMutational()
    {
    }

    private protected NonMutational(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected NonMutational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}