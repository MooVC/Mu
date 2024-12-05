namespace Mu.Architecture.Modelling;

public abstract record Creational
    : Mutational
{
    private protected Creational()
    {
    }

    private protected Creational(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected Creational(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}