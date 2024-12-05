namespace Mu.Architecture.Modelling;

public abstract record Mutational
    : UseCase
{
    private protected Mutational()
    {
    }

    private protected Mutational(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected Mutational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}