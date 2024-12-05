namespace Mu.Architecture.Modelling;

public abstract record UseCase
    : Causal
{
    private protected UseCase()
    {
    }

    private protected UseCase(DateTimeOffset proposed)
        : base(proposed)
    {
    }

    private protected UseCase(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}