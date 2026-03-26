namespace Mu.Modelling.Behavior;

/// <summary>
/// Represents a domain fact emitted as a consequence of a mutational use case.
/// </summary>
public abstract record Fact
    : Causal
{
    private protected Fact()
    {
    }

    private protected Fact(Guid identity, DateTimeOffset proposed)
        : base(identity, proposed)
    {
    }
}